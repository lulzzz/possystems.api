using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.ProductCategory
{
    public class CreateProductCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public bool TaxInd { get; set; }

        public bool SignatureReq { get; set; }
    }
}