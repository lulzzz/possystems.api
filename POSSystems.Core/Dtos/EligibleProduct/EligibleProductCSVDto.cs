namespace POSSystems.Core.Dtos
{
    public class EligibleProductCSVDto
    {
        public string UPC { get; set; }

        public string GTIN { get; set; }

        public string Description { get; set; }

        public string FLC { get; set; }

        public string CategoryDescription { get; set; }

        public string SubCategoryDescription { get; set; }

        public string FinestCategoryDescription { get; set; }

        public string ManufacturerName { get; set; }

        public string ChangeDate { get; set; }

        public string ChangeIndicator { get; set; }
    }
}