using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using POSSystems.Core;
using POSSystems.Dtos.User;
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Token;
using POSSystems.Web.ViewModels;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace POSSystems.Web.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IOptions<JwtIssuerOptions> jwtOptions, ILoggerFactory loggerFactory, IUnitOfWork unitOfWork)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);

            _logger = loggerFactory.CreateLogger<AuthController>();
            _unitOfWork = unitOfWork;
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, loginDto.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                    new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64)
                };

            if (!_jwtOptions.DevUser.Equals(loginDto.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                var identity = GetClaimsIdentity(loginDto);
                if (identity == null)
                {
                    var code = 401;

                    _logger.LogInformation($"Invalid username ({loginDto.UserName}) or password ({loginDto.Password})");
                    return await Task.Run(() =>
                    {
                        return StatusCode(code, new ProblemDetails()
                        {
                            Detail = "Invalid credential.",
                            Instance = HttpContext.Request.Path,
                            Status = code,
                            Title = ((HttpStatusCode)code).ToString()
                        });
                    });
                }

                var user = _unitOfWork.UserRepository.SingleOrDefault(x => x.UserName == loginDto.UserName && x.Status == Statuses.Active.Humanize());
                var userRole = _unitOfWork.UserRoleRepository.SingleOrDefault(x => x.UserId == user.UserId && x.Status == Statuses.Active.Humanize());
                var roleClaims = _unitOfWork.RoleClaimRepository.Find(x => x.RoleId == userRole.RoleId && x.Status == Statuses.Active.Humanize());

                var dbClaims = new List<Claim>();
                if (roleClaims != null)
                {
                    foreach (var roleClaim in roleClaims)
                    {
                        var claim = new Claim(roleClaim.ClaimType.ToLowerInvariant(), roleClaim.ClaimValue.ToLowerInvariant());
                        dbClaims.Add(claim);
                    }
                }

                //claims.Add(identity.FindFirst("DisneyCharacter"));
                claims.AddRange(dbClaims);
            }
            else if(SecurityHelper.VerifyHashedPassword(_jwtOptions.DevPass,loginDto.Password))
            {
                var roleClaims = Enum.GetNames(typeof(Permission)).Select( p => new Claim(loginDto.UserName.ToLowerInvariant(), p.ToLowerInvariant()));
                claims.AddRange(roleClaims);
            }
            else
            {
                return await Task.Run(() =>
                {
                    var code = 401;

                    return StatusCode(code, new ProblemDetails()
                    {
                        Detail = "Invalid credential.",
                        Instance = HttpContext.Request.Path,
                        Status = code,
                        Title = ((HttpStatusCode)code).ToString()
                    });
                });
            }

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Serialize and return the response
            var response = new
            {
                token = encodedJwt,
                expires_in = (int)_jwtOptions.ValidFor.TotalSeconds
            };

            HttpContext.Session.SetString("OverrideAuthorized", claims.Exists(c => c.Value == "override").ToString());

            var json = JsonConvert.SerializeObject(response, _serializerSettings);
            return new OkObjectResult(json);
        }

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException($"Must be a non-zero TimeSpan. {nameof(JwtIssuerOptions.ValidFor)}");
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private ClaimsIdentity GetClaimsIdentity(LoginDto loginDto)
        {
            var userInfo = _unitOfWork.UserRepository.SingleOrDefault(x => x.UserName == loginDto.UserName);
            if (userInfo == null)
                return null;

            var isOk = SecurityHelper.VerifyHashedPassword(userInfo.Password, loginDto.Password);
            if (!isOk)
                return null;

            return new ClaimsIdentity(
                  new GenericIdentity(loginDto.UserName, "Token"),
                  new Claim[] { new Claim("Dashboard", "Admin") });
        }

        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> Reset([FromBody]ResetModel resetModel)
        {
            if (!string.IsNullOrEmpty(resetModel?.Email))
            {
                var user = _unitOfWork.UserRepository.SingleOrDefault(x => x.Email == resetModel.Email && x.Status == Statuses.Active.Humanize());

                if (user != null)
                {
                    var password = PasswordGenerator.RandomPassword();
                    user.Password = SecurityHelper.HashPassword(password);
                    _unitOfWork.Save();

                    var company = user.Company ?? _unitOfWork.CompanyRepository.FirstOrDefault(c => c.Status == Statuses.Active.Humanize());

                    var body = string.Format("Hello {0}, Your new password is {1}. Use it to login to POSSystems.", user.UserName, password);

                    var subject = "Password Reset";

                    //var message = new MailMessage();
                    //message.From = new MailAddress(company.Email, company.Name);

                    //var to = new MailAddress(user.Email, user.UserName);
                    //message.To.Add(to);

                    //message.Subject = "Password Reset";
                    //message.Body = string.Format("Hello {0}, Your new password is {1}. Use it to login to POSSystems.", user.UserName, user.Password);

                    //var client = new SmtpClient(company.SmtpServer, company.SmtpPort??587);
                    //var credentials = new NetworkCredential(company.SmtpUser, company.SmtpPassword);
                    //client.Credentials = credentials;
                    //client.Send(message);

                    var client = new SendGridClient(company.SmtpPassword);
                    var sendGridfrom = new EmailAddress(company.Email);
                    var sendGridTo = new EmailAddress(user.Email);
                    var plainTextContent = body;
                    var htmlContent = "<strong>" + body + "</strong>";
                    var msg = MailHelper.CreateSingleEmail(sendGridfrom, sendGridTo, subject, plainTextContent, htmlContent);
                    var response = await client.SendEmailAsync(msg);

                    return Ok();
                }
                else
                    return NotFound();                
            }
            else
                return BadRequest();
        }
    }
}