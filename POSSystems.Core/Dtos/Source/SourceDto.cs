using POSSystems.Core.Dtos;

namespace POSSystems.Core.Models
{
    public class SourceDto : DtoBase
    {
        public int Id { get; set; }

        public string FileType { get; set; }

        public string HostAddress { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Port { get; set; }

        public string HostKey { get; set; }

        public string UploadPath { get; set; }

        public string DownloadPath { get; set; }

        public string Wildcard { get; set; }

        public string LocalPath { get; set; }

        public int SupplierId { get; set; }

        public string FieldSeperator { get; set; }

        public string SupplierName { get; set; }

        public string SubLocalPath { get; set; }

        public string ProcessingPath { get; set; }

        public string InterchangeSenderId { get; set; }

        public string InterchangeReceiverId { get; set; }

        public string EmployeeId { get; set; }

        public string VendorCustomerNo { get; set; }
    }
}