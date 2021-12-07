using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("batch_file")]
    public class BatchFile
    {
        [Key]
        [Column("file_id")]
        public int FileId { get; set; }

        [Required]
        [Column("file_name")]
        public string FileName { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; }

        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        [Column("error_count")]
        public int? ErrorCount { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }
    }
}