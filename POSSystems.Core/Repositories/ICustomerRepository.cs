using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Customer GetByLoyalCardNumber(string cardNumber);
    }
}