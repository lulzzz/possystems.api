using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class AccessLevelRepository : Repository<AccessLevel>
    , IAccessLevelRepository
    {
        public AccessLevelRepository(ApplicationDbContext context)
        : base(context)
        {
        }
    }
}