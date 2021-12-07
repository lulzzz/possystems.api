using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace POSSystems.Persistence.Repositories
{
    public class UserRepository : Repository<User>
    , IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public User GetByUsername(string username)
        {
            return Context.Set<User>().SingleOrDefault(u => u.UserName == username);
        }

        public User GetByEmail(string email)
        {
            return Context.Set<User>().Include(u => u.Company).SingleOrDefault(u => u.Email == email);
        }
    }
}