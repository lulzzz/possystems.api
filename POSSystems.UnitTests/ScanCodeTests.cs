using POSSystems.Core.Dtos.Product;
using Xunit;

namespace POSSystems.UnitTests
{
    public class ScanCodeTests
    {
        [Theory]
        [InlineData(null, null, null)]
        [InlineData("9999999", null, null)]
        [InlineData(null, "5", null)]
        [InlineData("241514452", "524213", null)]
        [InlineData("241", "5", null)]
        [InlineData("99999999999", "0", "99999999999")]
        [InlineData("09999999999", "1", "9999999999")]
        [InlineData("99999099999", "2", "9999999999")]
        [InlineData("99999999909", "3", "9999999999")]
        [InlineData("99999099999", "4", "9999999999")]
        [InlineData("99999999909", "5", "9999999999")]
        [InlineData("99999999990", "6", "9999999999")]
        [InlineData("09999999999", "7", "9999999999")]
        [InlineData("99999999909", "8", "9999999999")]
        public void Scancode_AllScenes_ProperCode(string upc_code, string cndcfi, string expected)
        {
            var actual = new ProductCSVDto2() { upc_code = upc_code, cndcfi = cndcfi };
            Assert.Equal(expected, actual.ScanCode);
        }
    }
}