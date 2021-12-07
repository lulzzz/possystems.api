using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class CompanyRepository : Repository<Company>
    , ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context)
        : base(context)
        {
        }
    }
}