namespace POSSystems.Core.Dtos.ProductCategory
{
    public class ProductCategoryDto : DtoBase
    {
        public int Id { get; set; }

        public string CategoryName { get; set; }

        public string Description { get; set; }

        public bool TaxInd { get; set; }

        public string TaxIndStr => TaxInd.ToString();

        public bool SignatureReq { get; set; }

        public string SignatureReqStr => SignatureReq.ToString();
    }
}