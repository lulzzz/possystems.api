using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class MeasurementUnitRepository : Repository<MeasurementUnit>
    , IMeasurementUnitRepository
    {
        public MeasurementUnitRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public bool Exists(string name)
        {
            return Context.Set<MeasurementUnit>().Any(mu => mu.MeasurementName == name);
        }

        public MeasurementUnit GetByName(string name)
        {
            return Context.Set<MeasurementUnit>().Where(mu => mu.MeasurementName == name).SingleOrDefault();
        }
    }
}