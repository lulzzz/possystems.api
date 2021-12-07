using POSSystems.Core.Dtos.Sales;
using POSSystems.Core.Models;
using System.Collections.Generic;

namespace POSSystems.Core.Repositories
{
    public interface ISalesMasterRepository : IRepository<SalesMaster>
    {
        int? GetLastInvoiceNo();

        void Add(SalesMaster salesMaster, Transaction transaction, List<PosItemDto> productsList, List<RxItemDto> rxList, double tax);

        SalesMaster GetByInvoiceNo(string invoiceNo, bool withDetail = false);
    }
}