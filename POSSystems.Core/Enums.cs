using System.ComponentModel;

namespace POSSystems.Core
{
    public enum Statuses
    {
        [Description("A")]
        Active = 1,

        [Description("I")]
        Inactive = 2
    }

    public enum TransactionType
    {
        [Description("S")]
        Sales = 1,

        [Description("P")]
        Purchase = 2,

        [Description("SR")]
        SalesReturn = 3,

        [Description("PR")]
        PurchaseReturn = 4
    }

    public enum PaymentStatus
    {
        [Description("C")]
        Complete = 1,

        [Description("P")]
        Partial = 2,

        [Description("N")]
        None = 3,

        [Description("PR")]
        PartiallyReturned = 4,

        [Description("CR")]
        CompletelyReturned = 5,

        [Description("A")]
        Active = 6,

        [Description("I")]
        Inactive = 7,

        [Description("NS")]
        NotSigned = 8,

        [Description("S")]
        Signed = 9
    }

    public enum PaymentType
    {
        FSA = 1,
        Card = 2,
        Cash = 3,
        Manual = 4,
        Bank = 5,
        Manualfsa = 6
    }

    public enum Mode
    {
        On = 1,
        Off = 2
    }

    public enum ItemType
    {
        [Description("RX")]
        RX = 1,

        [Description("PI")]
        PI = 2
    }

    public enum ResponseOrigin
    {
        [Description("TranCloud")]
        TranCloud = 1,

        [Description("Client")]
        Client = 2,

        [Description("Server")]
        Server = 3,

        [Description("Processor")]
        Processor = 4
    }

    public enum FileType
    {
        [Description("Edi832")]
        Edi832 = 1,

        [Description("Edi850")]
        Edi850 = 2,

        [Description("Edi855")]
        Edi855 = 3,

        [Description("CSV")]
        CSV = 4,

        [Description("ZIP")]
        ZIP = 5,
    }

    public enum FileStatus
    {
        [Description("D")]
        Downloaded = 1,

        [Description("S")]
        Started = 2,

        [Description("R")]
        Ready = 3,

        [Description("C")]
        Complete = 4,

        [Description("B")]
        BadFile = 5,

        [Description("P")]
        Partial = 6,

        [Description("F")]
        Failed = 7
    }

    public enum DeliveryStatus
    {
        [Description("Pending")]
        Pending = 1,

        [Description("Delivered")]
        Delivered = 2,

        [Description("Initialize")]
        Initialize = 3,

        [Description("Partial")]
        Partial = 4,

        [Description("Rejected")]
        Rejected = 5,
    }

    public enum Delivery855Status
    {
        [Description("Forwarded")]
        Forwarded = 1,

        [Description("Partial")]
        Partial = 2,

        [Description("Accepted")]
        Accepted = 3,

        [Description("Backordered")]
        Backordered = 4,

        [Description("QtyChanged")]
        QtyChanged = 5,

        [Description("Rejected")]
        Rejected = 6,

        [Description("Substitute")]
        Substitute = 7,

        [Description("OnHold")]
        OnHold = 8,
    }

    public enum CostPreference
    {
        [Description("Q")]
        Acquisition = 1,

        [Description("W")]
        AWP = 2
    }

    public enum SignatureStatus
    {
        [Description("N")]
        NotYet = 1,

        [Description("D")]
        Done = 2
    }
}