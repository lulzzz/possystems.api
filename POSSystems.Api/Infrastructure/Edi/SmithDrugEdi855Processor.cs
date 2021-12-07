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
    public class SmithDrugEdi855Processor : IEdiProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Edi855Job> _logger;
        private readonly JobConfigType _jobConfigType;

        private List<KeyValuePair<string, string>> statuses = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string,string>("IA","Accepted"),
            new KeyValuePair<string,string>("IQ","Qty Changed"),
            new KeyValuePair<string,string>("IS","Substitute"),
            new KeyValuePair<string,string>("IB","Backordered"),
            new KeyValuePair<string,string>("IR","Rejected"),
            new KeyValuePair<string,string>("AA","Forwarded"),
            new KeyValuePair<string,string>("BP","Partial"),
            new KeyValuePair<string,string>("IW", "On Hold" )
        };

        public SmithDrugEdi855Processor(IUnitOfWork unitOfWork, ILogger<Edi855Job> logger, JobConfigType jobConfigType)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jobConfigType = jobConfigType;
        }

        public void Process(string[] downloadedFiles, string ediPath, string inPath, string processingPath, int supplierId, string fieldSeparator)
        {
            var processedPath = Path.Combine(ediPath, processingPath);

            if (!string.IsNullOrEmpty(processingPath) && !Directory.Exists(processedPath))
                Directory.CreateDirectory(processedPath);

            var productsPurchased = AcknowledgementProcess(downloadedFiles, fieldSeparator, processedPath);

            foreach (var item in productsPurchased)
            {
                if (item.Value.Any())
                {
                    PurchaseMaster purchaseOrder = null;

                    foreach (var pitem in item.Value)
                    {
                        var product = _unitOfWork.ProductRepository.FirstOrDefault(s => s.UpcCode == pitem.UpcCode);
                        if (product != null)
                        {
                            if (purchaseOrder == null)
                            {
                                purchaseOrder = new PurchaseMaster()
                                {
                                    SupplierId = supplierId,
                                    PayMethod = "EDI855",
                                    Status = Statuses.Active.Humanize(),
                                    DeliveryStatus = DeliveryStatus.Pending.Humanize(),
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = DateTime.Now,
                                    CreatedBy = "JobUser",
                                    ModifiedBy = "JobUser",
                                    PurchaseMethod = "EDI855"
                                };

                                _unitOfWork.PurchaseMasterRepository.Add(purchaseOrder);

                                if (!_unitOfWork.Save())
                                    throw new Exception("Creating purchase failed on saving.");
                            }

                            var purchaseDetail = new PurchaseDetail()
                            {
                                PurchaseId = purchaseOrder.PurchaseId,
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

                            _unitOfWork.PurchaseDetailRepository.Add(purchaseDetail);
                            _unitOfWork.Save();
                            _logger.LogInformation($"Save order {purchaseOrder.PurchaseId}");
                        }
                        else
                        {
                            _logger.LogWarning($"EDI-855 {pitem.UpcCode} not found.");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("There is no item to save");
                }
            }
        }

        public Dictionary<int, List<PurchaseEdi850Model>> AcknowledgementProcess(string[] downloadedFiles, string fieldSeparator, string processedPath)
        {
            var productsPurchased = new Dictionary<int, List<PurchaseEdi850Model>>();

            foreach (string file in downloadedFiles)
            {
                BatchFile batchFile = null;
                try
                {
                    batchFile = _unitOfWork.BatchFileRepository.GetDownloadedBatchFile(file);
                    if (batchFile == null)
                        continue;

                    batchFile.Status = FileStatus.Started.Humanize();
                    if (!_unitOfWork.Save())
                    {
                        throw new LogException();
                    }

                    var purchaseOrders = new List<PurchaseEdi850Model>();
                    var eRxData = File.ReadAllText(file);
                    var segment = new List<string>(eRxData.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries));

                    var orderAcceptedDate = DateTime.Now;
                    int orderId = 0, errorCount = 0;

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
                                        string purchaseOrderNumber = fieldList[3];
                                        orderId = int.Parse(purchaseOrderNumber);
                                        string purchaseOrderDate = fieldList[4];
                                        orderAcceptedDate = DateTime.ParseExact(purchaseOrderDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                                    }
                                    break;

                                case "N1":
                                    {
                                        string accountNumber = fieldList[4];
                                    }
                                    break;

                                case "PO1":
                                    {
                                        string orderNumber = fieldList[1];
                                        string purchaseOrderQty = fieldList[2];
                                        string purchaseOrderUnitCode = fieldList[3];
                                        string purchaseOrderItemNumber = fieldList[9];
                                        string purchaseOrderNDCNumber = fieldList[9];
                                    }
                                    break;

                                case "ACK":
                                    {
                                        string statusCode = fieldList[1];
                                        string acknowledgementQty = fieldList[2];
                                        string acknowledgementUnitCode = fieldList[3];
                                        string acknowledgementDate = fieldList[5];

                                        string acknowledgementItemCode = fieldList[7];
                                        string acknowledgementItemNumber = fieldList[8];
                                        string acknowledgementNDCNumber = fieldList[10];

                                        var purchaseEdi850Model = new PurchaseEdi850Model
                                        {
                                            OrderId = orderId,
                                            ReorderUnits = int.Parse(acknowledgementQty),
                                            DeliveryQuantity = int.Parse(acknowledgementQty),
                                            VendorItemNo = int.Parse(acknowledgementItemNumber),
                                            UpcCode = acknowledgementNDCNumber,
                                            DeliveryStatus = statuses.FirstOrDefault(s => s.Key == statusCode).Value
                                        };

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

                    if (!string.IsNullOrEmpty(processedPath) && _jobConfigType == JobConfigType.Prod)
                    {
                        var destFile = Path.Combine(processedPath, Path.GetFileName(file));
                        if (File.Exists(destFile))
                            File.Delete(destFile);

                        File.Move(file, destFile);
                    }

                    if (purchaseOrders.Any())
                    {
                        if (errorCount == 0)
                        {
                            batchFile.Status = FileStatus.Complete.Humanize();
                            productsPurchased.Add(orderId, purchaseOrders);
                        }
                        else
                        {
                            batchFile.Status = FileStatus.Partial.Humanize();
                            batchFile.ErrorCount = errorCount;
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to extract any orders.");
                    }

                    if (!_unitOfWork.Save())
                    {
                        throw new LogException();
                    }
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
                    else
                        _logger.LogError(ex, ex.StackTrace);
                }
            }

            return productsPurchased;
        }
    }
}