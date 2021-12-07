namespace POSSystems.Core.Dtos.TranCloud
{
    public class Account
    {
        public string AcctNo;
    }

    public class TStream
    {
        public Transaction Transaction { get; set; }
    }

    public class Transaction
    {
        public string MerchantID;
        public string TranCode;
        public string InvoiceNo;
        public string RefNo;
        public string SecureDevice;
        public Amount Amount;
        public string TranDeviceID;
        public string PinPadIpPort;
        public string PinPadMACAddress;
        public string UserTrace;
        public string ComPort;
        public string SequenceNo;

        public string PartialAuth;
        public string Frequency;
        public string RecordNo;
        public Account Account;

        public string TranType;
        public string AuthCode;
        public string AcqRefData;
        public string FSAPrescription;
    }

    public class Amount
    {
        public string Purchase;
    }
}