namespace POSSystems.Core.Dtos.Transaction
{
    public class TransactionDto : DtoBase
    {
        public int Id { get; set; }

        public int? SalesId { get; set; }

        public int? PurchaseId { get; set; }

        public string PayMethod { get; set; }

        public string MaskedAcct { get; set; }

        public double Amount { get; set; }

        public string TransactionType { get; set; }

        public string CheckNo { get; set; }

        public string CardType { get; set; }

        public string Token { get; set; }

        public string Authcode { get; set; }

        public string AcqRefData { get; set; }
    }
}