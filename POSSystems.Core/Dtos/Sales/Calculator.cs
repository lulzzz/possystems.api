using POSSystems.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POSSystems.Core.Dtos.Sales
{
    public class Calculator
    {
        public Calculator()
        {
        }

        public Calculator(Models.Customer customer, List<Models.Product> products, List<RxItemDto> rxItems, double taxPercentage, int redeemThresholdPoint)
        {
            Customer = customer;
            Products = products;
            RxItems = rxItems;
            TaxPercentage = taxPercentage;
            RedeemThresholdPoint = redeemThresholdPoint;
        }

        public double POSItemsTotal { get; private set; } = 0;
        public double RxTotal { get; private set; } = 0;
        public double FsaTotal { get; private set; } = 0;
        public double TaxTotal { get; private set; } = 0;
        public double DiscountTotal { get; private set; } = 0;
        public double RedeemableAmount { get; private set; } = 0;
        public int PointsEarned { get; private set; } = 0;
        public int PointsRedeemed { get; private set; } = 0;
        public double TaxPercentage { get; private set; } = 0;
        public int RedeemThresholdPoint { get; private set; } = 0;
        public double ToBeProcessedTotal { get; private set; } = 0;

        public double Subtotal => POSItemsTotal + RxTotal;
        public double InvoicedTotal => Math.Round(POSItemsTotal + RxTotal + TaxTotal - DiscountTotal - RedeemableAmount, 2, MidpointRounding.AwayFromZero);
        public double RawTotal => POSItemsTotal + RxTotal + TaxTotal - DiscountTotal;

        public List<Models.Product> Products { get; set; }
        public List<RxItemDto> RxItems { get; set; }
        public Models.Customer Customer { get; private set; }

        public Calculator CalculateTotal(CreateSalesDto createSalesDto)
        {
            CalculatePOSItemsTotal(createSalesDto.PosItems, createSalesDto.DiscountPercentage);
            CalculateRxTotal(createSalesDto.RxList);

            CalculateLoyalty(createSalesDto.AddPointAndDoNotRedeem);

            if (RawTotal < createSalesDto.DiscountTotal)
            {
                throw new POSException("Discount is greater than total.", $"Discount is greater than total. On request:{createSalesDto.Total} and discount was {createSalesDto.DiscountTotal}");
            }

            if (Math.Abs(InvoicedTotal - createSalesDto.Total) > 0.01)
            {
                throw new POSException("Calculation mismatch.", $"Calculation mismatch. On request:{createSalesDto.Total} and calculated was {InvoicedTotal}");
            }

            return this;
        }

        public Calculator CalculatePayment(Models.SalesMaster salesMaster)
        {
            ToBeProcessedTotal = salesMaster?.Due ?? InvoicedTotal;

            return this;
        }

        private void CalculateLoyalty(bool? addPointAndDoNotRedeem)
        {
            if (Customer != null && addPointAndDoNotRedeem.HasValue)
            {
                CalculateEarnedPoints();

                if (!addPointAndDoNotRedeem.Value)
                {
                    CalculateRedemption();
                }

                Customer.LoyaltyPointEarned += PointsEarned - PointsRedeemed;
                Customer.DollarAmountSpend = (Customer.DollarAmountSpend ?? 0) + InvoicedTotal;
            }
        }

        public void CalculateRedemption()
        {
            if ((Customer.LoyaltyPointEarned ?? 0) < RedeemThresholdPoint) return;

            var pointDollarRatio = (Customer.PointDollarConversionRatio ?? 0) <= 0 ?
                                    throw new POSException("Point to Dollar Conversion Ratio is not set properly for this customer.") : Customer.PointDollarConversionRatio.Value;
            double dollarToGive = (Customer.LoyaltyPointEarned ?? 0) / pointDollarRatio;
            double actualDollarToGive = dollarToGive > RawTotal ? RawTotal : dollarToGive;

            PointsRedeemed = Convert.ToInt32(Math.Round(actualDollarToGive * pointDollarRatio, MidpointRounding.AwayFromZero));
            RedeemableAmount = Math.Round(actualDollarToGive, 2, MidpointRounding.AwayFromZero);
        }

        public void CalculateEarnedPoints()
        {
            var pointToGive = InvoicedTotal * (Customer.DollarPointConversionRatio ?? 0);
            PointsEarned = Convert.ToInt32(Math.Round(pointToGive, MidpointRounding.AwayFromZero));
        }

        public void CalculateRxTotal(List<RxItemDto> rxItemDtos)
        {
            for (int i = 0; i < rxItemDtos?.Count; i++)
            {
                var selectedRx = RxItems?.SingleOrDefault(r => r.RxNo == rxItemDtos[i].RxNo);
                if (selectedRx == null)
                {
                    throw new POSException($"Selected Rx not in the batch: {rxItemDtos[i]}");
                }
                else
                {
                    selectedRx.OverriddenPrice = rxItemDtos[i].OverriddenPrice;

                    FsaTotal += selectedRx.Copay.Value;
                    RxTotal += selectedRx.RealPrice;
                    TaxTotal += rxItemDtos[i].TaxAmount;
                }
            }
        }

        public void CalculatePOSItemsTotal(List<PosItemDto> posItemDtos, double? discountPercentage)
        {
            foreach (var item in Products)
            {
                var selectedItem = posItemDtos.SingleOrDefault(r => r.Id == item.ProductId);

                if (item.Quantity < selectedItem.Quantity)
                {
                    throw new POSException($"Product - {item.ProductName} is only available for {item.Quantity}.");
                }

                var itemTotalPrice = selectedItem.Quantity * (selectedItem.OverriddenPrice ?? item.SalesPrice);

                double totalItemDiscount = GetDiscount(selectedItem.DiscountItemPercentage, discountPercentage, itemTotalPrice);
                selectedItem.TotalItemDiscount = totalItemDiscount;
                selectedItem.AfterDiscountPrice = itemTotalPrice - totalItemDiscount;

                double itemTax = item.TaxIndicator ?? false ? GetTax(TaxPercentage, selectedItem.AfterDiscountPrice) : 0;

                if (selectedItem.IsFsa)
                {
                    FsaTotal += item.SalesPrice + itemTax;
                }

                POSItemsTotal += itemTotalPrice;
                DiscountTotal += totalItemDiscount;
                TaxTotal += itemTax;
            }
        }

        public double GetDiscount(double? discountItemPercentage, double? discountPercentage, double totalItemPrice)
        {
            double selectedDiscountPercentage = discountItemPercentage ?? discountPercentage ?? 0;
            var unroundedDiscount = (selectedDiscountPercentage * totalItemPrice) / 100;
            return Math.Round(unroundedDiscount, 2, MidpointRounding.AwayFromZero);
        }

        public double GetTax(double taxPercentage, double priceAfterDiscount)
        {
            var unroundedTax = (Math.Abs(taxPercentage) * priceAfterDiscount) / 100;
            return Math.Round(unroundedTax, 2, MidpointRounding.AwayFromZero);
        }
    }
}