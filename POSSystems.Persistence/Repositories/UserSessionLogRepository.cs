using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class UserSessionLogRepository : Repository<UserSessionLog>
    , IUserSessionLogRepository
    {
        public UserSessionLogRepository(ApplicationDbContext context)
        : base(context)
        {
        }
    }
}