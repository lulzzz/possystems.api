using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class PosTerminalRepository : Repository<PosTerminal>
    , IPosTerminalRepository
    {
        public PosTerminalRepository(ApplicationDbContext context)
        : base(context)
        {
        }
    }
}