using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using POSSystems.Core;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;

namespace POSSystems.Web.Controllers
{
    public abstract class BaseController<T> : Controller
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IUrlHelper _urlHelper;
        protected readonly ILogger<T> _logger;
        protected readonly IPropertyMappingService _propertyMappingService;
        protected readonly JsonSerializerSettings _serializerSettings;
        protected readonly IMapper _mapper;

        public BaseController(IUnitOfWork unitOfWork,
            ILogger<T> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._logger = logger;
            this._urlHelper = urlHelper;
            this._propertyMappingService = propertyMappingService;

            this._serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            _mapper = mapper;
        }

        //protected PaginationMetadata GeneratePaginationMetadata<TEntity>(PagedList<TEntity> pagedList, PageResourceParams pageResourceParams, string action)
        protected PaginationMetadata GeneratePaginationMetadata<TEntity>(PagedList<TEntity> pagedList)
        {
            //var previousPageLink = pagedList.HasPrevious ?
            //    CreatePagingResouceUri(pageResourceParams, ResourceUriType.PreviousPage, action) : null;

            //var nextPageLink = pagedList.HasNext ?
            //    CreatePagingResouceUri(pageResourceParams, ResourceUriType.NextPage, action) : null;

            var paginationMetadata = new PaginationMetadata
            {
                TotalCount = pagedList.TotalCount,
                Start = pagedList.Start,
                End = pagedList.End
            };

            return paginationMetadata;
        }

        //private string CreatePagingResouceUri(
        //    PageResourceParams pageResourceParams,
        //    ResourceUriType type,
        //    string action)
        //{
        //    var link = type switch
        //    {
        //        ResourceUriType.PreviousPage => _urlHelper.Link(action,
        //                new
        //                {
        //                    orderBy = pageResourceParams.OrderBy,
        //                    pageNumber = pageResourceParams.PageNumber - 1,
        //                    pageSize = pageResourceParams.PageSize
        //                }),

        //        ResourceUriType.NextPage => _urlHelper.Link(action,
        //                new
        //                {
        //                    orderBy = pageResourceParams.OrderBy,
        //                    pageNumber = pageResourceParams.PageNumber + 1,
        //                    pageSize = pageResourceParams.PageSize
        //                })
        //    };

        //    return link;
        //}

        //public TDestination MapEntity<TSource, TDestination>(TSource source, TDestination destination, bool forCreate = false) where TSource : class where TDestination : EntityBase
        //{
        //    TDestination returnValue;

        //    var userName = Request.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

        //    if (forCreate)
        //    {
        //        returnValue = _mapper.Map(source, destination,
        //            opt => opt.AfterMap((src, dest) =>
        //            {
        //                dest.CreatedBy = userName;
        //                dest.CreatedDate = DateTime.UtcNow;
        //                dest.ModifiedBy = userName;
        //                dest.ModifiedDate = DateTime.UtcNow;
        //                dest.Status = Statuses.Active.Humanize();
        //            }));
        //    }
        //    else
        //    {
        //        returnValue = _mapper.Map(source, destination,
        //            opt => opt.AfterMap((src, dest) =>
        //            {
        //                dest.ModifiedBy = userName;
        //                dest.ModifiedDate = DateTime.UtcNow;
        //            }));
        //    }

        //    return returnValue;
        //}
    }
}