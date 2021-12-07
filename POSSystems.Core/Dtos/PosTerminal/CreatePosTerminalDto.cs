using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.PosTerminal
{
    public class CreatePosTerminalDto
    {
        [Required]
        public int TerminalId { get; set; }

        [StringLength(50)]
        public string RefNo { get; set; }

        [StringLength(50)]
        public string PinpadIpPort { get; set; }

        [StringLength(50)]
        public string PinpadMacAddress { get; set; }

        [StringLength(50)]
        public string ComPort { get; set; }

        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(50)]
        public string TerminalName { get; set; }
    }
}