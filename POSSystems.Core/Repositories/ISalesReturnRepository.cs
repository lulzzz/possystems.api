using POSSystems.Core.Models;
using System.Collections.Generic;

namespace POSSystems.Core.Repositories
{
    public interface ISalesReturnRepository : IRepository<SalesReturn>
    {
        void Add(SalesMaster salesMaster, Customer customer,
            Transaction transaction, List<SalesReturn> salesReturns,
            string paymentStatus, double AmountAttended, double loyaltyAmountReturned);
    }
}