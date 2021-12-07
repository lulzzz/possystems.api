using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("source")]
    public class Source : EntityBase
    {
        [Key]
        [Column("source_id")]
        public int SourceID { get; set; }

        [Required]
        [StringLength(10)]
        [Column("file_type")]
        public string FileType { get; set; }

        [Required]
        [StringLength(100)]
        [Column("host_address")]
        public string HostAddress { get; set; }

        //[Required]
        [StringLength(50)]
        [Column("user_name")]
        public string UserName { get; set; }

        //[Required]
        [StringLength(50)]
        [Column("password")]
        public string Password { get; set; }

        //[Required]
        [Column("port")]
        public int Port { get; set; }

        //[Required]
        [StringLength(100)]
        [Column("host_key")]
        public string HostKey { get; set; }

        //[Required]
        [StringLength(100)]
        [Column("upload_path")]
        public string UploadPath { get; set; }

        //[Required]
        [StringLength(100)]
        [Column("download_path")]
        public string DownloadPath { get; set; }

        //[Required]
        [StringLength(50)]
        [Column("wildcard")]
        public string Wildcard { get; set; }

        //[Required]
        [StringLength(100)]
        [Column("local_path")]
        public string LocalPath { get; set; }

        [Required]
        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        [StringLength(10)]
        [Column("field_seperator")]
        public string FieldSeperator { get; set; }

        [StringLength(50)]
        [Column("sub_local_path")]
        public string SubLocalPath { get; set; }

        [StringLength(50)]
        [Column("processing_path")]
        public string ProcessingPath { get; set; }

        [StringLength(50)]
        [Column("interchange_senderId")]
        public string InterchangeSenderId { get; set; }

        [StringLength(50)]
        [Column("interchange_receiverId")]
        public string InterchangeReceiverId { get; set; }

        [StringLength(50)]
        [Column("employee_id")]
        public string EmployeeId { get; set; }

        [StringLength(50)]
        [Column("vendor_customer_no")]
        public string VendorCustomerNo { get; set; }
    }
}