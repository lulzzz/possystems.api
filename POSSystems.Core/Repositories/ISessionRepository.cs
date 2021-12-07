using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface ISessionRepository : IRepository<Session>
    {
        Session GetLastSessionByUsername(string username);
    }
}