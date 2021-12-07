namespace POSSystems.Core.Dtos.PurchaseReturn
{
    public class PurchaseReturnDto : DtoBase
    {
        public int Id { get; set; }

        public int PurchaseId { get; set; }

        public int ProductId { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; }

        public double ReturnAmount { get; set; }

        public string ReturnType { get; set; }

        public int? ReasonId { get; set; }

        public string Reason { get; set; }

        public string Product { get; set; }
    }
}