namespace POSSystems.Core.Dtos.Return
{
    public class PaymentableDto
    {
        public double Total { get; set; }
        public string PaymentType { get; set; }
        public string Token { get; set; }
    }
}