using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    public abstract class EntityBase
    {
        [Required, Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Required, Column("created_by"), StringLength(50)]
        public string CreatedBy { get; set; }

        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [Column("modified_by"), StringLength(50)]
        public string ModifiedBy { get; set; }

        [Required, Column("status"), StringLength(1)]
        public string Status { get; set; }
    }
}