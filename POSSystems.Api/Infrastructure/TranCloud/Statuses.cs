namespace POSSystems.Web.Infrastructure.TranCloud
{
    public enum TransactionStatus
    {
        NotStarted = 0,
        Approved = 1,
        Error = 2,
        Declined = 3,
        DeclinedOffline = 4
    }
}