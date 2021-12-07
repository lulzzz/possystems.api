using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface IConfigurationRepository : IRepository<Configuration>
    {
        string GetConfigByKey(string key);

        string GetConfigByKey(string key, string defaultValue);
    }
}