using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class DiscountCouponRepository : Repository<DiscountCoupon>
    , IDiscountCouponRepository
    {
        public DiscountCouponRepository(ApplicationDbContext context)
        : base(context)
        {
        }
    }
}