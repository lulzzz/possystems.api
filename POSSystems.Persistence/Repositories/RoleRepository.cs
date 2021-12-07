using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class RoleRepository : Repository<Role>
    , IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context)
        : base(context)
        {
        }
    }
}