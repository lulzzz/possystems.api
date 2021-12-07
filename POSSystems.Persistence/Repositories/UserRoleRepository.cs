using Microsoft.EntityFrameworkCore;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace POSSystems.Persistence.Repositories
{
    public class UserRoleRepository : Repository<UserRole>
    , IUserRoleRepository
    {
        public UserRoleRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public new UserRole Get(int id)
        {
            return Context.Set<UserRole>().Include(p => p.Role).Include(p => p.User).FirstOrDefault(x => x.UserRoleId == id);
        }

        public new IQueryable<UserRole> GetAllDeferred(Expression<Func<UserRole, bool>> predicate)
        {
            return Context.Set<UserRole>().Where(predicate).Include(p => p.Role).Include(p => p.User);
        }

        public new IQueryable<UserRole> GetAllDeferred()
        {
            return Context.Set<UserRole>().Include(p => p.Role).Include(p => p.User);
        }
    }
}