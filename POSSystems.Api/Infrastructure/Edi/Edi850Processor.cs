using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.PurchaseMaster;
using POSSystems.Core.Models;
using POSSystems.Web.Controllers;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace POSSystems.Web.Infrastructure.Edi
{
    /// <summary>
    /// Edi850Processor
    /// </summary>
    public class Edi850Processor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private ILogger<PurchaseExportController> _logger;

        public Edi850Processor(IUnitOfWork unitOfWork, IWebHostEnvironment env, ILogger<PurchaseExportController> logger)
        {
            this._unitOfWork = unitOfWork;
            _hostingEnvironment = env;
            _logger = logger;
            //  _logger = logger;//, ILogger<Edi850Processor>  logger
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="purchaseOrders"></param>
        public void Process(int supplierId, List<PurchaseEdi850Model> purchaseOrders)
        {
            #region -- [ Get soruce info ] --

            var soruceInfo = _unitOfWork.SourceRepository.Find(s => s.SupplierId == supplierId && s.FileType == FileType.Edi850.Humanize()).FirstOrDefault();
            //var temSource = new TmpSource();

            #endregion -- [ Get soruce info ] --

            #region -- [ Save Purchase info ] --

            // Get purchase Details
            var prodectDetailsUpcs = purchaseOrders.Select(s => s.UpcCode).ToList();
            var prodectDetails = _unitOfWork.ProductRepository.GetAllDeferred(s => prodectDetailsUpcs.Contains(s.UpcCode)).ToList();
            var purchaseMaster = new PurchaseMaster() { SupplierId = supplierId, Status = Statuses.Active.Humanize(), DeliveryStatus = DeliveryStatus.Initialize.Humanize() };

            // add purchaseMaster
            _unitOfWork.PurchaseMasterRepository.Add(purchaseMaster);
            if (!_unitOfWork.Save())
                throw new Exception($"Creating purchase failed on saving.");

            // add purchaseDetail
            foreach (var item in purchaseOrders)
            {
                var product = prodectDetails.Where(s => s.UpcCode == item.UpcCode).FirstOrDefault();
                if (product != null)
                {
                    var purchaseDetail = new PurchaseDetail()
                    {
                        PurchaseId = purchaseMaster.PurchaseId,
                        ProductId = product.ProductId,
                        UpcScanCode = product.UpcScanCode,
                        Quantity = item.ReorderUnits,
                        //MeasurementId = productDetail.MeasurementId,
                        //MeasurementUnit = productDetail.MeasurementUnit,
                        Price = item.ReorderUnits * product.PurchasePrice,
                        DeliveryStatus = DeliveryStatus.Initialize.Humanize(),
                        Status = Statuses.Active.Humanize(),
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        CreatedBy = "JobUser",
                        ModifiedBy = "JobUser"
                    };
                    item.OrderId = purchaseMaster.PurchaseId;

                    _unitOfWork.PurchaseDetailRepository.Add(purchaseDetail);
                }
            }

            if (!_unitOfWork.Save())
                throw new Exception($"Creating purchase Details failed on saving.");
            //var temSource = new TmpSource();

            #endregion -- [ Save Purchase info ] --

            #region -- [ Generate Files ] --

            var str = GetHeaderData(soruceInfo, purchaseMaster.PurchaseId);
            var itemNo = 0;
            foreach (var item in purchaseOrders)
            {
                str += GetDetail(item, ++itemNo);
            }
            //TODO: Get emplyee id and no of segments
            str += GetTrailer(purchaseOrders.Count, soruceInfo.EmployeeId, purchaseOrders.Count + 6);

            #endregion -- [ Generate Files ] --

            #region -- [ Save file in directory ] --

            var filePath = "850";//ger it from wildecard field

            string processedPath = Path.Combine(soruceInfo.LocalPath, filePath);
            var filename = processedPath + "/edi850_" + Guid.NewGuid() + ".X12";
            //string processedPath = Path.Combine(_hostingEnvironment.ContentRootPath, filePath);
            if (!string.IsNullOrEmpty(processedPath) && !Directory.Exists(processedPath))
                Directory.CreateDirectory(processedPath);
            var file = new System.IO.StreamWriter(filename);

            file.WriteLine(str);
            file.Close();

            #endregion -- [ Save file in directory ] --

            #region -- [ Upload File ] --

            Upload(soruceInfo.HostAddress, soruceInfo.UserName, soruceInfo.Password, soruceInfo.Port, soruceInfo.HostKey, soruceInfo.UploadPath, filename);

            #endregion -- [ Upload File ] --
        }

        /// <summary>
        /// GetHeaderData
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public string GetHeaderData(Source source, int orderId)
        {
            string fieldSeparator = source.FieldSeperator;// "|";
            string segmentSeparator = "~";

            DateTime date = DateTime.Now;

            string isa = "ISA" + fieldSeparator;
            string isa01 = "00" + fieldSeparator;
            string isa02 = new string(' ', 10) + fieldSeparator;
            string isa03 = "00" + fieldSeparator;
            string isa04 = new string(' ', 10) + fieldSeparator;
            string isa05 = "ZZ" + fieldSeparator;
            string isa06 = "" + source.InterchangeSenderId.PadRight(15, ' ') + fieldSeparator; // Interchange Sender ID  ---- Padding with 15 length
            string isa07 = "01" + fieldSeparator;
            string isa08 = "" + source.InterchangeReceiverId.PadRight(15, ' ') + fieldSeparator; // Interchange Receiver ID  ---- Padding with 15 length
            string isa09 = date.ToString("yyMMdd") + fieldSeparator;
            string isa10 = date.ToString("HHmm") + fieldSeparator;
            string isa11 = "U" + fieldSeparator;
            string isa12 = "00401" + fieldSeparator;
            string isa13 = "123456789" + fieldSeparator;
            string isa14 = "0" + fieldSeparator;
            string isa15 = "P" + fieldSeparator;
            string isa16 = ">" + segmentSeparator;

            string gs = "GS" + fieldSeparator;
            string gs01 = "PO" + fieldSeparator;
            string gs02 = "" + source.InterchangeSenderId + fieldSeparator; // Interchange Sender ID
            string gs03 = "" + source.InterchangeReceiverId + fieldSeparator; // Interchange Receiver ID
            string gs04 = date.ToString("yyyyMMdd") + fieldSeparator;
            string gs05 = date.ToString("HHmm") + fieldSeparator;
            string gs06 = "111111111" + fieldSeparator;
            string gs07 = "X" + fieldSeparator;
            string gs08 = "004010" + segmentSeparator;

            string st = "ST" + fieldSeparator;
            string st01 = "850" + fieldSeparator;
            string st02 = "" + source.EmployeeId + segmentSeparator; // Pharmacy Employee ID

            string beg = "BEG" + fieldSeparator;
            string beg01 = "00" + fieldSeparator;
            string beg02 = "NE" + fieldSeparator;
            string beg03 = "" + orderId + fieldSeparator; // Purchase Order Number
            string beg04 = "" + fieldSeparator;
            string beg05 = date.ToString("yyyyMMdd") + segmentSeparator;

            string strRef = "REF" + fieldSeparator;
            string ref01 = "CO" + fieldSeparator;
            string ref02 = "123456" + segmentSeparator;

            string n1 = "N1" + fieldSeparator;
            string n101 = "BY" + fieldSeparator;
            string n102 = "" + fieldSeparator;
            string n103 = "91" + fieldSeparator;
            string n104 = "" + source.VendorCustomerNo + segmentSeparator;  // Vendor Customer No

            return isa + isa01 + isa02 + isa03 + isa04 + isa05 + isa06 + isa07 + isa08 + isa09 + isa10 + isa11 + isa12 + isa13 + isa14 + isa15 + isa16 +
                    gs + gs01 + gs02 + gs03 + gs04 + gs05 + gs06 + gs07 + gs08 +
                    st + st01 + st02 +
                    beg + beg01 + beg02 + beg03 + beg04 + beg05 +
                    strRef + ref01 + ref02 +
                    n1 + n101 + n102 + n103 + n104;
        }

        /// <summary>
        /// GetDetail purchaseOrder
        /// </summary>
        /// <param name="purchaseOrder"></param>
        /// <returns></returns>
        public string GetDetail(PurchaseEdi850Model purchaseOrder, int itemNo)
        {
            //PO1|1|1|UN|||VN|685798~
            var strDetails = "";// "\n";

            //strDetails += "\n";
            string fieldSeparator = "|";
            string segmentSeparator = "~";

            string po1 = "PO1" + fieldSeparator;
            string po101 = "" + itemNo + fieldSeparator; // Order Number
            string po102 = "" + purchaseOrder.ReorderUnits + fieldSeparator; // Quantity Orderred
            string po103 = "UN" + fieldSeparator;
            string po104 = "" + fieldSeparator;
            string po105 = "" + fieldSeparator;
            string po106 = "VN" + fieldSeparator;
            string po107 = "" + purchaseOrder.VendorItemNo + segmentSeparator; // Vendor Item No

            if (po107.Length == 0)
            {
                po106 = "N4" + fieldSeparator;
                po107 = "" + purchaseOrder.UpcCode + segmentSeparator; // UPC CodecurOrder.cNDC && Product / Service ID
            }

            strDetails += po1 + po101 + po102 + po103 + po104 + po105 + po106 + po107;

            return strDetails;
        }

        /// <summary>
        /// GetTrailer
        /// </summary>
        /// <param name="noTransactionLineItem"></param>
        /// <param name="employeeId"></param>
        /// <param name="noOfSegment"></param>
        /// <returns></returns>
        public string GetTrailer(int noTransactionLineItem, string employeeId, int noOfSegment)
        {
            string fieldSeparator = "|";
            string segmentSeparator = "~";

            DateTime date = DateTime.Now;

            string ctt = "CTT" + fieldSeparator;
            string ctt01 = "" + noTransactionLineItem + segmentSeparator; // Number of Transaction Line Item

            string se = "SE" + fieldSeparator;
            string se02 = "" + employeeId + segmentSeparator; // Pharmacy Employee Id

            string ge = "GE" + fieldSeparator;
            string ge01 = "1" + fieldSeparator;
            string ge02 = "111111111" + segmentSeparator;

            string iea = "IEA" + fieldSeparator;
            string iea01 = "1" + fieldSeparator;
            string iea02 = "123456789" + segmentSeparator;

            string se01 = "" + noOfSegment + fieldSeparator; // no.of segments being Added

            return ctt + ctt01 +
                    se + se01 + se02 +
                    ge + ge01 + ge02 +
                    iea + iea01 + iea02;
        }

        /// <summary>
        /// Upload
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="port"></param>
        /// <param name="hostKey"></param>
        /// <param name="uploadPath"></param>
        /// <param name="localFilePath"></param>
        public void Upload(string hostAddress, string userName, string password, int port, string hostKey, string uploadPath, string localFilePath)
        {
            //var connectionInfo = new ConnectionInfo(hostAddress,
            //                            port,
            //                            userName,
            //                            new PasswordAuthenticationMethod(userName, password));
            var connectionInfo = new ConnectionInfo(hostAddress,
                                        port,
                                        userName,
                                        new PasswordAuthenticationMethod(userName, password),
                                        new PrivateKeyAuthenticationMethod(hostKey));
            using (var client = new SftpClient(connectionInfo))
            {
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(60);
                try
                {
                    client.Connect();
                    _logger.LogInformation($"Connected to {hostAddress}.");

                    var f = new FileInfo(localFilePath);
                    string uploadfile = f.FullName;
                    //Console.WriteLine(f.Name);
                    //Console.WriteLine("uploadfile" + uploadfile);

                    var fileStream = new FileStream(uploadfile, FileMode.Open);
                    if (fileStream == null)
                    {
                        _logger.LogError($"File can not be null");
                    }

                    client.BufferSize = 4 * 1024;
                    client.UploadFile(fileStream, uploadPath + f.Name, null);
                    client.Disconnect();
                    client.Dispose();
                    _logger.LogInformation($"File {uploadfile} uploaded to {uploadPath}.");
                }
                catch (SshAuthenticationException sae)
                {
                    _logger.LogError($"Not able to login to {hostAddress} because {sae.Message}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Not able to import data from Edi 850 source {hostAddress} for {ex.Message}.");
                }
            }
        }

        //private void UpdateProgresBar(ulong uploaded)
        //{
        //    // Update progress bar on foreground thread
        //    progressBar1.Invoke(
        //        (MethodInvoker)delegate { progressBar1.Value = (int)uploaded; });
        //    _logger.LogError($"Not able to login to {hostAddress} because {sae.Message}.");
        //}
    }
}