using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        User GetByUsername(string username);
        User GetByEmail(string email);
    }
}