using Humanizer;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.PurchaseMaster;
using POSSystems.Core.Exceptions;
using POSSystems.Core.Models;
using POSSystems.Web.Infrastructure.Jobs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace POSSystems.Web.Infrastructure.Edi
{
    public class Edi855Processor : IEdiProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Edi855Job> _logger;

        private List<KeyValuePair<string, string>> Status = new List<KeyValuePair<string, string>>()
               {
                        new KeyValuePair<string,string>("AA","Forwarded"),

                        new KeyValuePair<string,string>("BP","Partial"),

                        new KeyValuePair<string,string>("IA","Accepted"),

                        new KeyValuePair<string,string>("IB","Backordered"),

                        new KeyValuePair<string,string>("IQ","Qty Changed"),

                        new  KeyValuePair<string,string>("IR","Rejected"),

                        new KeyValuePair<string,string>("IS","Substitute"),

                        new KeyValuePair<string,string>("IW", "On Hold" )
                    };

        public Edi855Processor(IUnitOfWork unitOfWork, ILogger<Edi855Job> logger)
        {
            this._unitOfWork = unitOfWork;
            this._logger = logger;
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="downloadedFiles"></param>
        /// <param name="ediPath"></param>
        /// <param name="inPath"></param>
        /// <param name="processingPath"></param>
        /// <param name="fieldSeparator"></param>
        /// <param name="sourceInfo"></param>
        public void Process(string[] downloadedFiles, string ediPath, string inPath, Source sourceInfo)
        {
            // Get download files from local folder if there is now download files
            if (downloadedFiles == null)
                downloadedFiles = Directory.GetFiles(Path.Combine(ediPath, inPath));

            // Process Download Files
            var productDetails = AcknowledgementProcess(downloadedFiles, sourceInfo);
            // Update Purchase Master

            #region

            foreach (var item in productDetails)
            {
                // system purchase order
                var purchaseMaster = _unitOfWork.PurchaseMasterRepository.Get(item.Key);
                if (purchaseMaster != null)
                {
                    if (purchaseMaster.DeliveryStatus == DeliveryStatus.Initialize.Humanize())
                    {
                        var purchaseDetails = _unitOfWork.PurchaseDetailRepository.GetAllDeferred(s => s.PurchaseId == purchaseMaster.PurchaseId).ToList();

                        foreach (var pitem in item.Value)
                        {
                            var purchaselist = purchaseDetails.Where(s => s.Product.ItemNo == pitem.VendorItemNo.ToString());
                            if (purchaselist != null && purchaselist.Any())
                            {
                                foreach (var purchasedetail in purchaselist)
                                {
                                    purchasedetail.DeliveredQuantity = pitem.DeliveryQuantity;
                                    purchasedetail.DeliveryStatus = pitem.DeliveryStatus;
                                }
                            }

                            //productDetail.Quantity += pitem.ReorderUnits;
                            _unitOfWork.Save();
                        }
                        purchaseMaster.DeliveryStatus = DeliveryStatus.Pending.Humanize();
                        _unitOfWork.Save();
                    }
                }
                else
                {
                    if (item.Value.Any())
                    {
                        // new purchase order
                        var newPurchase = new PurchaseMaster()
                        {
                            SupplierId = sourceInfo.SupplierId,
                            Status = Statuses.Active.Humanize(),
                            DeliveryStatus = DeliveryStatus.Pending.Humanize(),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            CreatedBy = "JobUser",
                            ModifiedBy = "JobUser"
                        };

                        // add purchaseMaster
                        _unitOfWork.PurchaseMasterRepository.Add(newPurchase);
                        if (!_unitOfWork.Save())
                            throw new Exception($"Creating purchase failed on saving.");

                        // add purchaseDetail
                        foreach (var pitem in item.Value)
                        {
                            var product = _unitOfWork.ProductRepository.FirstOrDefault(s => s.ItemNo == pitem.VendorItemNo.ToString());
                            if (product != null)
                            {
                                var purchaseDetail = new PurchaseDetail()
                                {
                                    PurchaseId = newPurchase.PurchaseId,
                                    UpcScanCode = product.UpcScanCode,
                                    ProductId = product.ProductId,
                                    Quantity = pitem.ReorderUnits,
                                    Price = pitem.ReorderUnits * product.PurchasePrice,
                                    DeliveryStatus = pitem.DeliveryStatus,
                                    DeliveredQuantity = pitem.DeliveryQuantity,
                                    Status = Statuses.Active.Humanize(),
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = DateTime.Now,
                                    CreatedBy = "JobUser",
                                    ModifiedBy = "JobUser"
                                };
                                //item.OrderId = newPurchase.PurchaseId;

                                _unitOfWork.PurchaseDetailRepository.Add(purchaseDetail);
                                _unitOfWork.Save();
                                _logger.LogInformation($"Save order {newPurchase.PurchaseId}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"There is no item to save");
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Acknowledgement file  Process
        /// </summary>
        /// <param name="downloadedFiles"></param>
        /// <param name="sourceInfo"></param>
        /// <returns></returns>
        public Dictionary<int, List<PurchaseEdi850Model>> AcknowledgementProcess(string[] downloadedFiles, Source sourceInfo)
        {
            string[] files = downloadedFiles;

            string processedPath = Path.Combine(sourceInfo.LocalPath, sourceInfo.ProcessingPath);

            if (!string.IsNullOrEmpty(processedPath) && !Directory.Exists(processedPath))
                Directory.CreateDirectory(processedPath);

            string fieldSeparator = sourceInfo.FieldSeperator;
            var productDetails = new Dictionary<int, List<PurchaseEdi850Model>>();

            foreach (string file in files)
            {
                BatchFile batchFile = null;
                try
                {
                    #region Parse file and generate model

                    batchFile = _unitOfWork.BatchFileRepository.GetDownloadedBatchFile(file);
                    if (batchFile == null)
                    {
                        continue;
                    }
                    batchFile.Status = FileStatus.Started.Humanize();
                    if (!_unitOfWork.Save())
                    {
                        throw new LogException();
                    }

                    var purchaseOrders = new List<PurchaseEdi850Model>();

                    string eRxData = File.ReadAllText(file);

                    var segment = new List<string>(eRxData.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries));

                    // var productDetail = new ProductDetail();

                    DateTime orderAcceptedDate = DateTime.Now;
                    int orderId = 0;
                    int errorCount = 0;
                    foreach (var item in segment)
                    {
                        try
                        {
                            var fieldList = new List<string>(item.Split(fieldSeparator).ToList());
                            string segmentName = fieldList[0];
                            switch (segmentName)
                            {
                                case "BAK":

                                    {
                                        //Todo:ask rob vai
                                        string purchaseOrderNumber = fieldList[3];

                                        orderId = int.Parse(purchaseOrderNumber);
                                        //string date = fieldList[4];

                                        string purchaseOrderDate = fieldList[4];
                                        orderAcceptedDate = DateTime.ParseExact(purchaseOrderDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                                    }

                                    break;

                                case "N1":

                                    {
                                        string accountNumber = fieldList[3];
                                    }

                                    break;

                                case "PO1":

                                    {
                                        string orderNumber = fieldList[1];

                                        string purchaseOrderQty = fieldList[2];

                                        string purchaseOrderUnitCode = fieldList[3];

                                        string purchaseOrderUnitPrice = fieldList[4];

                                        string purchaseOrderItemCode = fieldList[6];

                                        string purchaseOrderItemNumber = fieldList[7];
                                        string purchaseOrderNDCCode = "", purchaseOrderNDCNumber = "";

                                        if (fieldList.Count > 8)
                                        {
                                            purchaseOrderNDCCode = fieldList[8];
                                        }
                                        if (fieldList.Count > 9)
                                        {
                                            purchaseOrderNDCNumber = fieldList[8];
                                        }
                                        //string purchaseOrderNDCCode = fieldList[8];

                                        //string purchaseOrderNDCNumber = fieldList[9];
                                    }

                                    break;

                                case "ACK":

                                    {
                                        string statusCode = fieldList[1]; // Get this data from above list code

                                        string acknowledgementQty = fieldList[2];
                                        //productDetail.Quantity = int.Parse(acknowledgementQty);

                                        string acknowledgementUnitCode = fieldList[3];
                                        //productDetail.UpcCode = acknowledgementUnitCode;

                                        string acknowledgementItemCode = fieldList[7];
                                        // purchaseDetail.Quantity = acknowledgementQty;

                                        string acknowledgementItemNumber = fieldList[8];

                                        if (fieldList.Count > 9)
                                        {
                                            string acknowledgementNDCCode = fieldList[9];
                                            //productDetail.UpcCode = acknowledgementNDCCode;
                                        }
                                        if (fieldList.Count > 10)
                                        {
                                            string acknowledgementNDCNumber = fieldList[10];
                                            // productDetail.UpcCode = acknowledgementNDCNumber;
                                        }

                                        var purchaseEdi850Model = new PurchaseEdi850Model();
                                        purchaseEdi850Model.OrderId = orderId;
                                        purchaseEdi850Model.ReorderUnits = int.Parse(acknowledgementQty);
                                        purchaseEdi850Model.DeliveryQuantity = int.Parse(acknowledgementQty);
                                        purchaseEdi850Model.VendorItemNo = int.Parse(acknowledgementItemNumber);
                                        //purchaseEdi850Model.VendorItemNo = acknowledgementItemCode;
                                        //if (statusCode.Contains("IB"))
                                        //{
                                        //    purchaseEdi850Model.DeliveryStatus = Status.FirstOrDefault(s => s.Key == statusCode).Value;
                                        //}
                                        //else if (statusCode.Contains("IA"))
                                        //{
                                        //    purchaseEdi850Model.DeliveryStatus = DeliveryStatus.Pending.Humanize();
                                        //}
                                        //else
                                        //{
                                        //    purchaseEdi850Model.DeliveryStatus = DeliveryStatus.Rejected.Humanize();
                                        //}
                                        purchaseEdi850Model.DeliveryStatus = Status.FirstOrDefault(s => s.Key == statusCode).Value;
                                        purchaseOrders.Add(purchaseEdi850Model);
                                    }

                                    break;
                            }
                        }
                        catch (POSException pox)
                        {
                            errorCount++;

                            if (!_unitOfWork.Save())
                            {
                                _logger.LogError(pox.UserMessage);
                                _logger.LogError($"Was not able to log error for {orderId}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;

                            if (!_unitOfWork.Save())
                            {
                                _logger.LogError(ex.Message);
                                _logger.LogError($"Was not able to log error for {orderId}.");
                            }
                        }
                    }
                    //productDetails.Add(orderId, purchaseOrders);

                    #endregion

                    #region MovepProcess  file to processed path

                    if (!string.IsNullOrEmpty(processedPath))
                    {
                        var destFile = Path.Combine(processedPath, Path.GetFileName(file));
                        if (File.Exists(destFile))
                            File.Delete(destFile);

                        File.Move(file, destFile);
                    }

                    if (errorCount == 0)
                    {
                        batchFile.Status = FileStatus.Complete.Humanize();
                        productDetails.Add(orderId, purchaseOrders);
                    }
                    else
                    {
                        batchFile.Status = FileStatus.Partial.Humanize();
                        batchFile.ErrorCount = errorCount;
                    }

                    if (!_unitOfWork.Save())
                    {
                        throw new LogException();
                    }

                    batchFile = null;

                    #endregion
                }
                catch (LogException)
                {
                    _logger.LogError($"Status of file {batchFile.FileName} failed to update.");
                }
                catch (Exception ex)
                {
                    if (batchFile != null)
                    {
                        batchFile.Status = FileStatus.BadFile.Humanize();
                        if (!_unitOfWork.Save())
                        {
                            _logger.LogError($"Status of file {batchFile.FileName} failed to update.");
                        }
                    }
                    _logger.LogError(ex, ex.StackTrace);
                }
            }
            return productDetails;
        }

        public void Process(string[] downloadedFiles, string ediPath, string inPath, string processingPath, int supplierId, string fieldSeparator)
        {
            throw new NotImplementedException();
        }
    }
}