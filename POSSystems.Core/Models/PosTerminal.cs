using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("pos_terminal")]
    public class PosTerminal : EntityBase
    {
        [Key]
        [Required]
        [Column("terminal_id")]
        public int TerminalId { get; set; }

        [Required]
        [Column("terminal_name")]
        public string TerminalName { get; set; }

        [StringLength(50)]
        [Column("ref_no")]
        public string RefNo { get; set; }

        [StringLength(50)]
        [Column("pinpad_ip_port")]
        public string PinpadIpPort { get; set; }

        [StringLength(50)]
        [Column("pinpad_mac_address")]
        public string PinpadMacAddress { get; set; }

        [StringLength(50)]
        [Column("com_port")]
        public string ComPort { get; set; }

        [Required]
        [StringLength(50)]
        [Column("ip_address")]
        public string IpAddress { get; set; }
    }
}