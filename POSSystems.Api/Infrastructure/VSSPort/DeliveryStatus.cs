namespace POSSystems.Web.Infrastructure.VSSPort
{
    public static class DeliveryStatus
    {
        public const string UNDELIVERED = "01";
        public const string ATTEMPTED = "02";
        public const string REFUSED = "03";
        public const string DELIVERED = "04";
        public const string PICKEDUP = "05";
        public const string PARTIAL = "06";
        public const string VERIFICATIONPENDING = "07";
        public const string VERIFIED = "99"; // FOR DELIVERY APP, WE USE THIS PROPERTY IN OFFLINE FUNCTIONALITY
    }
}