using AutoMapper;
using POSSystems.Core.Models;
using System.Collections.Generic;

namespace POSSystems.Core.Repositories
{
    public interface IEligibleProductRepository : IRepository<EligibleProduct>
    {
        IMapper RepMapper { get; set; }

        EligibleProduct Get(string upc);

        void Merge(IEnumerable<EligibleProduct> eligibleProducts);

        bool Exists(string upc);
    }
}