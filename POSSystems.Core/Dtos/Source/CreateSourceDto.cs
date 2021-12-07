using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Models
{
    public class CreateSourceDto
    {
        [Required]
        [MaxLength(10)]
        public string FileType { get; set; }

        [Required]
        [MaxLength(100)]
        public string HostAddress { get; set; }

        //[Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        //[Required]
        [MaxLength(50)]
        public string Password { get; set; }

        //[Required]
        public int Port { get; set; }

        //[Required]
        [MaxLength(100)]
        public string HostKey { get; set; }

        //[Required]
        [StringLength(100)]
        public string UploadPath { get; set; }

        //[Required]
        [MaxLength(100)]
        public string DownloadPath { get; set; }

        //[Required]
        [MaxLength(50)]
        public string Wildcard { get; set; }

        [Required]
        [MaxLength(100)]
        public string LocalPath { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [MaxLength(10)]
        public string FieldSeperator { get; set; }

        [MaxLength(50)]
        public string SubLocalPath { get; set; }

        [MaxLength(50)]
        public string ProcessingPath { get; set; }

        [MaxLength(50)]
        public string InterchangeSenderId { get; set; }

        [MaxLength(50)]
        public string InterchangeReceiverId { get; set; }

        [MaxLength(50)]
        public string EmployeeId { get; set; }

        [MaxLength(50)]
        public string VendorCustomerNo { get; set; }
    }
}