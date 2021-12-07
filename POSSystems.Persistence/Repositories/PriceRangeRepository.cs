using Humanizer;
using Microsoft.EntityFrameworkCore;
using POSSystems.Core;
using POSSystems.Core.Exceptions;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class PriceRangeRepository : Repository<PriceRange>
    , IPriceRangeRepository
    {
        public PriceRangeRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public new IQueryable<PriceRange> GetAllDeferred()
        {
            return Context.Set<PriceRange>().Include(p => p.ProductPriceRange);
        }

        public double SelectPrice(Product product, PriceCatalog priceCatalog)
        {
            var result = double.TryParse(priceCatalog.PurchasePrice, out double purchasePrice);

            if (product?.ProductPriceRange == null && result)
                return purchasePrice;
            else if (product?.ProductPriceRange == null && !result)
                throw new POSException("Acquisition price is not parseable.");

            if (double.TryParse(priceCatalog.SalesPrice, out double salesPrice))
            {
                throw new POSException("Preferred cost is salesprice which is not parseable.");
            }

            return GetCalculatedPrice(product, salesPrice, purchasePrice);
        }

        public double GetCalculatedPrice(Product product, double salesPrice, double purchasePrice)
        {
            if (!product.ProductPriceRangeId.HasValue)
                return salesPrice;

            if (product?.ProductPriceRange == null)
            {
                product.ProductPriceRange = Context.Set<ProductPriceRange>().Where(prr => prr.ProductPriceRangeId == product.ProductPriceRangeId).SingleOrDefault();
            }

            var costPreference = product?.ProductPriceRange.CostPreference.DehumanizeTo<CostPreference>() ?? throw new POSException("Cost Preference not set.");

            double preferredPrice = costPreference switch
            {
                CostPreference.Acquisition => purchasePrice,
                CostPreference.AWP => salesPrice
            };

            var selectedRange = Context.Set<PriceRange>()
                .Where(pr => pr.ProductPriceRangeId == product.ProductPriceRangeId && pr.MaxRange >= preferredPrice && pr.MinRange <= preferredPrice)
                .OrderBy(pr => pr.MinRange).FirstOrDefault();

            return CalculateRetail(preferredPrice, selectedRange == null ? 1 : selectedRange.Markup);
        }

        private double CalculateRetail(double cost, double markup)
        {
            //var percAmt = cost * (percent / 100);
            //var finCost = cost + percAmt;

            return Math.Round(cost * markup, 2, MidpointRounding.AwayFromZero);
        }
    }
}