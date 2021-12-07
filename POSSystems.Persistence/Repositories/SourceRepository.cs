using Microsoft.EntityFrameworkCore;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace POSSystems.Persistence.Repositories
{
    public class SourceRepository : Repository<Source>, ISourceRepository
    {
        public SourceRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public new IQueryable<Source> GetAllDeferred()
        {
            return Context.Set<Source>().Include(p => p.Supplier);
        }

        public new IQueryable<Source> GetAllDeferred(Expression<Func<Source, bool>> predicate)
        {
            return Context.Set<Source>().Where(predicate).Include(p => p.Supplier);
        }
    }
}