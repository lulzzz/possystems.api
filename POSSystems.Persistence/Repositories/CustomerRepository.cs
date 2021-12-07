using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public Customer GetByLoyalCardNumber(string cardNumber)
        {
            return Context.Set<Customer>().Where(c => c.LoyaltyCardNumber == cardNumber).SingleOrDefault();
        }
    }
}