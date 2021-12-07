using POSSystems.Core.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace POSSystems.Core.Repositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        new UserRole Get(int id);

        new IQueryable<UserRole> GetAllDeferred(Expression<Func<UserRole, bool>> predicate);

        new IQueryable<UserRole> GetAllDeferred();
    }
}