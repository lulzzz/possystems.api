namespace POSSystems.Core.Dtos.PosTerminal
{
    public class PosTerminalDto : DtoBase
    {
        public int Id { get; set; }
        public string RefNo { get; set; }
        public string PinpadIpPort { get; set; }
        public string PinpadMacAddress { get; set; }
        public string ComPort { get; set; }
        public string IpAddress { get; set; }
        public string TerminalName { get; set; }
    }
}