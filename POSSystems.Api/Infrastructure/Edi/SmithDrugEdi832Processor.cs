using Humanizer;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Exceptions;
using POSSystems.Core.Models;
using POSSystems.Web.Infrastructure.Jobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace POSSystems.Web.Infrastructure.Edi
{
    public class SmithDrugEdi832Processor : IEdiProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Edi832Job> _logger;
        private readonly JobConfigType _jobConfigType;

        public SmithDrugEdi832Processor(IUnitOfWork unitOfWork, ILogger<Edi832Job> logger, JobConfigType jobConfigType)
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

            //var listQualifier = new List<string> { "N1", "N2", "N3", "N4" };

            foreach (string file in downloadedFiles)
            {
                BatchFile batchFile = null;
                try
                {
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

                    var eRxData = File.ReadAllText(file);
                    var segment = new List<string>(eRxData.Split(new string[] { "~" },
                                                   StringSplitOptions.RemoveEmptyEntries));

                    if (!string.IsNullOrEmpty(processingPath) && _jobConfigType == JobConfigType.Prod)
                    {
                        var destFile = Path.Combine(processedPath, Path.GetFileName(file));
                        if (File.Exists(destFile))
                            File.Delete(destFile);

                        File.Move(file, destFile);
                    }

                    var priceCatalog = new PriceCatalog
                    {
                        FileId = batchFile.FileId,
                        SupplierId = supplierId
                    };

                    var errorCount = 0;
                    foreach (var item in segment)
                    {
                        try
                        {
                            var fieldList = new List<string>(item.Split(fieldSeparator).ToList());
                            string segmentName = fieldList[0].Trim();
                            switch (segmentName)
                            {
                                case "LIN":
                                    {
                                        priceCatalog.ItemNo = fieldList[3].Trim();
                                        priceCatalog.ProductQualifier = fieldList[4].Trim();
                                        priceCatalog.UPCCode = fieldList[5].Trim();
                                        priceCatalog.ScanCode = fieldList[5].Remove(0, 1).Trim();
                                    }
                                    break;

                                case "PID":
                                    {
                                        priceCatalog.Category = fieldList[2].Trim();
                                        priceCatalog.CategoryDescription = CategoryDescriptions(fieldList[2]).Trim();
                                        priceCatalog.ProductName = fieldList[5].Trim();
                                    }
                                    break;

                                case "P4":
                                    priceCatalog.PackageSize = fieldList[1].Trim();
                                    break;

                                case "CTP":
                                    {
                                        if (fieldList[2].Trim() == "NET")
                                            priceCatalog.PurchasePrice = decimal.Parse(fieldList[3]).ToString("F2").Trim();

                                        if (fieldList[2].Trim() == "AWP")
                                            priceCatalog.SalesPrice = decimal.Parse(fieldList[3]).ToString("F2").Trim();
                                    }
                                    break;
                            }

                            if (segmentName == "CTP" && fieldList[2].Trim() == "AWP")
                            {
                                _unitOfWork.PriceCatalogRepository.Add(priceCatalog);
                                if (!_unitOfWork.Save())
                                {
                                    throw new POSException($"EDI832- Inserting {priceCatalog.ProductName} failed in {batchFile.FileName}.");
                                }
                                else
                                {
                                    InsertFromPriceCatalog(priceCatalog);
                                }

                                priceCatalog = new PriceCatalog
                                {
                                    FileId = batchFile.FileId,
                                    SupplierId = supplierId
                                };
                            }
                        }
                        catch (POSException pox)
                        {
                            LogEdiError(supplierId, batchFile, ref priceCatalog, ref errorCount, pox.UserMessage);
                        }
                        catch (Exception ex)
                        {
                            LogEdiError(supplierId, batchFile, ref priceCatalog, ref errorCount, ex.Message);
                        }
                    }

                    if (errorCount == 0)
                    {
                        batchFile.Status = FileStatus.Complete.Humanize();
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
                }
                catch (LogException)
                {
                    _logger.LogError($"EDI832- Status of file {batchFile.FileName} failed to update.");
                }
                catch (Exception ex)
                {
                    if (batchFile != null)
                    {
                        batchFile.Status = FileStatus.BadFile.Humanize();
                        if (!_unitOfWork.Save())
                        {
                            _logger.LogError($"EDI832- Status of file {batchFile.FileName} failed to update.");
                        }
                    }
                    _logger.LogError(ex, ex.StackTrace);
                }
            }
        }

        private void LogEdiError(int supplierId, BatchFile batchFile, ref PriceCatalog priceCatalog, ref int errorCount, string error)
        {
            errorCount++;
            priceCatalog.Error = error;
            _unitOfWork.TrySave(out string message);

            _logger.LogError(error);
            _logger.LogError($"EDI832- Was not able to log error for {priceCatalog.UPCCode} in {priceCatalog.FileId}.");

            priceCatalog = new PriceCatalog
            {
                FileId = batchFile.FileId,
                SupplierId = supplierId
            };
        }

        private void InsertFromPriceCatalog(PriceCatalog priceCatalog)
        {
            var product = _unitOfWork.ProductRepository.GetByRealUpc(priceCatalog.UPCCode ?? throw new POSException("UPCCode not found in Edi"));

            if (product == null)
            {
                var exists = _unitOfWork.ProductRepository.ExistsScanUpc(priceCatalog.ScanCode ?? throw new POSException("UPCScanCode not found in Edi"));
                if (exists)
                {
                    throw new POSException($"EDI832- Scan code already exists for inserting in product {priceCatalog.ScanCode}.");
                }

                var productCategory = _unitOfWork.ProductCategoryRepository.Get(priceCatalog.Category ?? throw new POSException("Category not found in Edi"));
                if (productCategory == null)
                {
                    productCategory = new ProductCategory { CategoryName = priceCatalog.Category, Description = priceCatalog.CategoryDescription };
                    _unitOfWork.ProductCategoryRepository.Add(productCategory);
                    if (!_unitOfWork.Save())
                    {
                        throw new POSException($"EDI832- Unable to insert category {priceCatalog.Category}.");
                    }
                }

                priceCatalog.Unit = "EA";
                var measurementUnit = _unitOfWork.MeasurementUnitRepository.GetByName(priceCatalog.Unit);
                if (measurementUnit == null)
                {
                    measurementUnit = new MeasurementUnit { MeasurementName = priceCatalog.Unit };
                    _unitOfWork.MeasurementUnitRepository.Add(measurementUnit);
                    if (!_unitOfWork.Save())
                    {
                        throw new POSException($"EDI832- Unable to insert measurement {priceCatalog.Unit}.");
                    }
                }

                var salesPrice = Helpers.StringExtensions.ReturnNullableDouble(priceCatalog.SalesPrice);
                var purchasePrice = Helpers.StringExtensions.ReturnNullableDouble(priceCatalog.PurchasePrice);
                var packageSize = Helpers.StringExtensions.ReturnNullableInt(priceCatalog.PackageSize);

                product = new Product
                {
                    ProductName = priceCatalog.ProductName,
                    Description = priceCatalog.ProductDescription,
                    CategoryId = productCategory.CategoryID,
                    UpcCode = priceCatalog.UPCCode,
                    UpcScanCode = priceCatalog.ScanCode,
                    SupplierId = priceCatalog.SupplierId,
                    MeasurementId = measurementUnit.MeasurementId,
                    PackageSize = packageSize,
                    PurchasePrice = purchasePrice,
                    Strength = priceCatalog.Strength,
                    SalesPrice = salesPrice ?? 0,
                    ItemNo = priceCatalog.ItemNo
                };

                _unitOfWork.ProductRepository.Add(product);
                if (!_unitOfWork.Save())
                {
                    throw new POSException($"EDI832- Unable to insert product {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                }

                priceCatalog.Imported = true;
                if (!_unitOfWork.Save())
                {
                    _logger.LogWarning($"EDI832- Unable to update price catalog {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                }
            }
            else
            {
                var salesPrice = Helpers.StringExtensions.ReturnNullableDouble(priceCatalog.SalesPrice);
                var purchasePrice = Helpers.StringExtensions.ReturnNullableDouble(priceCatalog.PurchasePrice);

                product.PurchasePrice = purchasePrice;
                product.SalesPrice = salesPrice ?? 0;

                priceCatalog.Imported = true;
                if (!_unitOfWork.Save())
                {
                    _logger.LogError($"EDI832- Unable to update price catalog {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                }
            }
        }

        public static string CategoryDescriptions(string value)
        {
            var returnValue = value switch
            {
                "HB" => "Health And Beauty Aids",
                "MS" => "Medical Supplies",
                "OT" => "Over The Counter",
                "RX" => "Prescription",
                "C2" => "Schedule II",
                "C3" => "Schedule III",
                "C4" => "Schedule IV",
                "C5" => "Schedule V"
            };

            return returnValue;
        }
    }
}