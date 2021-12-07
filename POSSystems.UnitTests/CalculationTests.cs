using POSSystems.Core.Dtos.Sales;
using POSSystems.Core.Exceptions;
using POSSystems.Core.Models;
using System.Collections.Generic;
using Xunit;

namespace POSSystems.UnitTests
{
    public class CalculationTests
    {
        private readonly List<Core.Models.Product> _products;
        private readonly List<PosItemDto> _posItems;

        public CalculationTests()
        {
            _products = new List<Core.Models.Product>();
            _posItems = new List<PosItemDto>();
        }

        [Theory]
        [InlineData(10, 100, 10)]
        [InlineData(11.1, 99, 10.99)]
        [InlineData(0, 100, 0)]
        [InlineData(-1, 100, 1)]
        public void GetTax_AllScenes_ReturnsTax(double taxPercentage, double afterDiscountPrice, double expected)
        {
            var actual = new Calculator().GetTax(taxPercentage, afterDiscountPrice);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, null, 10, 0)]
        [InlineData(10, null, 100, 10)]
        [InlineData(null, 10, 100, 10)]
        [InlineData(11, 10, 100, 11)]
        [InlineData(11.1, 10, 99, 10.99)]
        public void GetDiscount_AllScenes_ReturnsDiscount(double? discountPercentage, double? discountItemPercentage, double totalItemPrice, double expected)
        {
            var actual = new Calculator().GetDiscount(discountPercentage, discountItemPercentage, totalItemPrice);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CalculatePOSItemsTotal_WhenTaxIndFalse_DontUpdateTax()
        {
            _products.Add(new Product { ProductId = 1, ProductName = "Test1", TaxInd = false, Quantity = 5, SalesPrice = 100, });

            _posItems.Add(new PosItemDto { Id = 1, Quantity = 2 });

            var calculator = new Calculator(null, _products, null, 10, 0);
            calculator.CalculatePOSItemsTotal(_posItems, 10);

            Assert.Equal(200, calculator.POSItemsTotal);
            Assert.Equal(0, calculator.TaxTotal);
            Assert.Equal(20, calculator.DiscountTotal);
        }

        [Fact]
        public void CalculatePOSItemsTotal_WhenBuyingMoreThanStock_ThrowsException()
        {
            _products.Add(new Product { ProductId = 1, ProductName = "Test1", TaxInd = false, Quantity = 5, SalesPrice = 100, });

            _posItems.Add(new PosItemDto { Id = 1, Quantity = 6 });

            var calculator = new Calculator(null, _products, null, 10, 0);
            var ex = Assert.Throws<POSException>(() => calculator.CalculatePOSItemsTotal(_posItems, 10));

            Assert.Contains("is only available for", ex.UserMessage);
        }

        [Fact]
        public void CalculatePOSItemsTotal_WhenMultipleItems_CalculatesCorrectTotal()
        {
            _products.Add(new Product { ProductId = 1, ProductName = "Test1", TaxInd = true, Quantity = 10, SalesPrice = 100, });
            _products.Add(new Product { ProductId = 2, ProductName = "Test2", TaxInd = true, Quantity = 5, SalesPrice = 10, });

            _posItems.Add(new PosItemDto { Id = 1, Quantity = 9 });
            _posItems.Add(new PosItemDto { Id = 2, Quantity = 3 });

            var calculator = new Calculator(null, _products, null, 10, 0);
            calculator.CalculatePOSItemsTotal(_posItems, 10);

            Assert.Equal(930, calculator.POSItemsTotal);
            Assert.Equal(83.7, calculator.TaxTotal);
            Assert.Equal(93, calculator.DiscountTotal);
        }

        [Fact]
        public void CalculatePOSItemsTotal_WhenItemDiscount_CalculatesCorrectTotal()
        {
            _products.Add(new Product { ProductId = 1, ProductName = "Test1", TaxInd = true, Quantity = 10, SalesPrice = 100, });
            _products.Add(new Product { ProductId = 2, ProductName = "Test2", TaxInd = true, Quantity = 5, SalesPrice = 10, });

            _posItems.Add(new PosItemDto { Id = 1, Quantity = 9, DiscountItemPercentage = 15 });
            _posItems.Add(new PosItemDto { Id = 2, Quantity = 3 });

            var calculator = new Calculator(null, _products, null, 10, 0);
            calculator.CalculatePOSItemsTotal(_posItems, 10);

            Assert.Equal(930, calculator.POSItemsTotal);
            Assert.Equal(138, calculator.DiscountTotal);
            Assert.Equal(79.2, calculator.TaxTotal);
        }
    }
}