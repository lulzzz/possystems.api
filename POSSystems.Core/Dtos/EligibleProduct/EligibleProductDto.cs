namespace POSSystems.Core.Dtos.EligibleProduct
{
    public class EligibleProductDto : DtoBase
    {
        public string Id { get; set; }

        public string Gtin { get; set; }

        public string Description { get; set; }

        public string Flc { get; set; }

        public string CategoryDescription { get; set; }

        public string SubCategoryDescription { get; set; }

        public string FinestCategoryDescription { get; set; }

        public string ManufacturerName { get; set; }

        public string ChangeDate { get; set; }

        public string ChangeIndicator { get; set; }
    }
}