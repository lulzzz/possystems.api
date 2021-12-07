using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface IMeasurementUnitRepository : IRepository<MeasurementUnit>
    {
        MeasurementUnit GetByName(string name);

        bool Exists(string name);
    }
}