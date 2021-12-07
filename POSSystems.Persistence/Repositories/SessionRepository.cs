using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System;

namespace POSSystems.Persistence.Repositories
{
    public class SessionRepository : Repository<Session>
    , ISessionRepository
    {
        public SessionRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public new IQueryable<Session> GetAllDeferred(Expression<Func<Session, bool>> predicate)
        {
            return Context.Set<Session>().Include(p => p.User).Where(predicate);
        }

        public Session GetLastSessionByUsername(string username)
        {
            return Context.Set<Session>().Include(s => s.User).Where(s => s.User.UserName == username).OrderByDescending(s => s.StartTime).FirstOrDefault();
        }
    }
}