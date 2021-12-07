using Microsoft.EntityFrameworkCore;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace POSSystems.Persistence.Repositories
{
    public class RoleClaimRepository : Repository<RoleClaim>, IRoleClaimRepository
    {
        public RoleClaimRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public new IQueryable<RoleClaim> GetAllDeferred(Expression<Func<RoleClaim, bool>> predicate)
        {
            return Context.Set<RoleClaim>().Where(predicate).Include(p => p.Role);
        }

        public new IQueryable<RoleClaim> GetAllDeferred()
        {
            return Context.Set<RoleClaim>().Include(p => p.Role);
        }
    }
}