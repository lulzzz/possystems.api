using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class ManufacturerRepository : Repository<Manufacturer>
    , IManufacturerRepository
    {
        public ManufacturerRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public Manufacturer GetByName(string name)
        {
            return Context.Set<Manufacturer>().Where(m => m.Name == name).SingleOrDefault();
        }
    }
}