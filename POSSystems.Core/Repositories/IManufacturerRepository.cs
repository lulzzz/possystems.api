using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface IManufacturerRepository : IRepository<Manufacturer>
    {
        Manufacturer GetByName(string name);
    }
}