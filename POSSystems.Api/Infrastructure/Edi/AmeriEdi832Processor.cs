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
using System.Text.RegularExpressions;

namespace POSSystems.Web.Infrastructure.Edi
{
    public class AmeriEdi832Processor : IEdiProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Edi832Job> _logger;

        public AmeriEdi832Processor(IUnitOfWork unitOfWork, ILogger<Edi832Job> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public void Process(string[] downloadedFiles, string ediPath, string inPath, string processingPath, int supplierId, string fieldSeparator)
        {
            string processedPath = Path.Combine(ediPath, processingPath);

            if (!string.IsNullOrEmpty(processingPath) && !Directory.Exists(processedPath))
                Directory.CreateDirectory(processedPath);

            var listQualifier = new List<string> { "N1", "N2", "N3" };

            if (downloadedFiles == null)
                downloadedFiles = Directory.GetFiles(Path.Combine(ediPath, inPath));

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

                    string eRxData = File.ReadAllText(file);

                    var segment = new List<string>(eRxData.Split(new string[] { "~" },
                                                   StringSplitOptions.RemoveEmptyEntries));

                    if (!string.IsNullOrEmpty(processingPath))
                    {
                        var destFile = Path.Combine(processedPath, Path.GetFileName(file));
                        if (File.Exists(destFile))
                            File.Delete(destFile);

                        File.Move(file, destFile);
                    }

                    var priceCatalog = new PriceCatalog { FileId = batchFile.FileId, SupplierId = supplierId };

                    int errorCount = 0;
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
                                        priceCatalog.ProductQualifier = fieldList[6].Trim();
                                        priceCatalog.UPCCode = fieldList[7].Trim();
                                        priceCatalog.ScanCode = fieldList[13].Remove(0, 1).Trim();
                                        priceCatalog.ItemNo = fieldList[3].Trim();
                                    }

                                    break;

                                case "REF":
                                    if (fieldList[1].Trim() == "2K")
                                    {
                                        priceCatalog.Category = fieldList[2].Trim();
                                        priceCatalog.CategoryDescription = CategoryDescriptions(fieldList[2]).Trim();
                                    }

                                    break;

                                case "PID":
                                    {
                                        if ((fieldList[1].Trim() == "S") && (fieldList[2].Trim().Length == 0))
                                            priceCatalog.ProductName = Regex.Replace(fieldList[5], @"\s+", " ").Replace("\"", "", StringComparison.InvariantCultureIgnoreCase).Replace(",", "", StringComparison.InvariantCultureIgnoreCase).Trim();

                                        if ((fieldList[1].Trim() == "S") && (fieldList[2].Trim() == "GEN"))
                                        {
                                            priceCatalog.ProductId = Regex.Replace(fieldList[4], @"\s+", " ").Replace("\"", "", StringComparison.InvariantCultureIgnoreCase).Replace(",", "", StringComparison.InvariantCultureIgnoreCase).Trim();
                                            priceCatalog.ProductDescription = Regex.Replace(fieldList[5], @"\s+", " ").Replace("\"", "", StringComparison.InvariantCultureIgnoreCase).Replace(",", "", StringComparison.InvariantCultureIgnoreCase).Trim();
                                        }

                                        if ((fieldList[1].Trim() == "F") && (fieldList[2].Trim() == "DF"))
                                            priceCatalog.Form = fieldList[5].Trim();

                                        if ((fieldList[1].Trim() == "F") && (fieldList[2].Trim() == "FMR"))
                                            priceCatalog.Strength = fieldList[5].Trim();
                                    }

                                    break;

                                case "PO4":
                                    if (fieldList.Count > 3)
                                    {
                                        priceCatalog.Unit = fieldList[3].Trim();
                                        priceCatalog.PackageSize = fieldList[2].Trim();
                                    }

                                    break;

                                case "CTP":
                                    {
                                        if (fieldList[2].Trim() == "CON")
                                            if (!string.IsNullOrEmpty(fieldList[3].Trim()))
                                                priceCatalog.PurchasePrice = decimal.Parse(fieldList[3]).ToString("F2").Trim();

                                        if (!string.IsNullOrEmpty(fieldList[3].Trim()))
                                            if (fieldList[2].Trim() == "AWP")
                                                priceCatalog.SalesPrice = decimal.Parse(fieldList[3]).ToString("F2").Trim();
                                    }

                                    break;

                                case "N1":
                                    {
                                        if (fieldList[1].Trim() == "SU")
                                            priceCatalog.Manufacturer = fieldList[2].Replace("\"", "", StringComparison.InvariantCultureIgnoreCase).Replace(",", "", StringComparison.InvariantCultureIgnoreCase).Trim();
                                    }

                                    break;
                            }

                            if (segmentName == "G39")
                            {
                                if (listQualifier.Contains(priceCatalog.ProductQualifier))
                                {
                                    _unitOfWork.PriceCatalogRepository.Add(priceCatalog);

                                    if (!_unitOfWork.Save())
                                    {
                                        throw new POSException($"Inserting {priceCatalog.ProductName} failed in {batchFile.FileName}.");
                                    }
                                    else
                                    {
                                        InsertFromPriceCatalog(priceCatalog);
                                    }

                                    priceCatalog = new PriceCatalog { FileId = batchFile.FileId, SupplierId = supplierId };
                                }
                            }
                        }
                        catch (POSException pox)
                        {
                            errorCount++;
                            priceCatalog.Error = pox.UserMessage;
                            if (!_unitOfWork.Save())
                            {
                                _logger.LogError(pox.UserMessage);
                                _logger.LogError($"Was not able to log error for {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                            }
                            priceCatalog = new PriceCatalog { FileId = batchFile.FileId, SupplierId = supplierId };
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            priceCatalog.Error = ex.Message;
                            if (!_unitOfWork.Save())
                            {
                                _logger.LogError(ex.Message);
                                _logger.LogError($"Was not able to log error for {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                            }
                            priceCatalog = new PriceCatalog { FileId = batchFile.FileId, SupplierId = supplierId };
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
        }

        private void InsertFromPriceCatalog(PriceCatalog priceCatalog)
        {
            string catalogProductId = priceCatalog.ProductId;
            //if (int.TryParse(priceCatalog.ProductId, out catalogProductId))
            //{
            var product = _unitOfWork.ProductRepository.GetProductByCatalog(catalogProductId);

            if (product == null)
            {
                var productCategory = _unitOfWork.ProductCategoryRepository.Get(priceCatalog.Category);

                if (productCategory == null)
                {
                    productCategory = new ProductCategory { CategoryName = priceCatalog.Category, Description = priceCatalog.CategoryDescription };
                    _unitOfWork.ProductCategoryRepository.Add(productCategory);
                    if (!_unitOfWork.Save())
                    {
                        throw new POSException($"Unable to insert category {priceCatalog.Category}.");
                    }
                }

                product = new Product
                {
                    ProductName = priceCatalog.ProductName,
                    Description = priceCatalog.ProductDescription,
                    CatalogProductId = catalogProductId,
                    CategoryId = productCategory.CategoryID
                };

                _unitOfWork.ProductRepository.Add(product);
                if (!_unitOfWork.Save())
                {
                    throw new POSException($"Unable to insert product {priceCatalog.ProductId}.");
                }
            }

            var productDetail = _unitOfWork.ProductRepository.GetByRealUpc(priceCatalog.UPCCode);

            if (productDetail == null)
            {
                if (!_unitOfWork.ProductRepository.ExistsScanUpc(priceCatalog.ScanCode))
                {
                    var selectedPrice = _unitOfWork.PriceRangeRepository.SelectPrice(product, priceCatalog);

                    var measurementUnit = _unitOfWork.MeasurementUnitRepository.GetByName(priceCatalog.Unit);
                    if (measurementUnit == null)
                    {
                        measurementUnit = new MeasurementUnit { MeasurementName = priceCatalog.Unit };
                        _unitOfWork.MeasurementUnitRepository.Add(measurementUnit);

                        if (!_unitOfWork.Save())
                        {
                            throw new POSException($"Unable to insert measurement {priceCatalog.Unit}.");
                        }
                    }

                    int? nPackageSize = Helpers.StringExtensions.ReturnNullableInt(priceCatalog.PackageSize);
                    double? nPurchasePrice = Helpers.StringExtensions.ReturnNullableDouble(priceCatalog.PurchasePrice);

                    productDetail = new Product
                    {
                        UpcCode = priceCatalog.UPCCode,
                        UpcScanCode = priceCatalog.ScanCode,
                        SupplierId = priceCatalog.SupplierId,
                        MeasurementId = measurementUnit.MeasurementId,
                        PackageSize = nPackageSize,
                        ProductId = product.ProductId,
                        PurchasePrice = nPurchasePrice,
                        Strength = priceCatalog.Strength,
                        SalesPrice = selectedPrice,
                        ItemNo = priceCatalog.ItemNo
                    };

                    _unitOfWork.ProductRepository.Add(productDetail);
                    if (!_unitOfWork.Save())
                    {
                        throw new POSException($"Unable to insert product detail {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                    }

                    priceCatalog.Imported = true;
                    if (!_unitOfWork.Save())
                    {
                        _logger.LogInformation($"Unable to update price catalog {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                    }
                }
                else
                {
                    throw new POSException($"Unable to insert product detail {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                }
            }
            else
            {
                var selectedPrice = _unitOfWork.PriceRangeRepository.SelectPrice(product, priceCatalog);
                double? nPurchasePrice = Helpers.StringExtensions.ReturnNullableDouble(priceCatalog.PurchasePrice);

                productDetail.PurchasePrice = nPurchasePrice;
                productDetail.SalesPrice = selectedPrice;

                priceCatalog.Imported = true;
                if (!_unitOfWork.Save())
                {
                    _logger.LogInformation($"Unable to update price catalog {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                }

                //Commented because client wants to update price
                //priceCatalog.Error = "Already Exists";
                //if (!_unitOfWork.Save())
                //{
                //    _logger.LogInformation($"Price update for {priceCatalog.UPCCode} ignored.");
                //}
            }
            //}
            //else
            //{
            //    _logger.LogError($"product id - { priceCatalog.ProductId } not parseable in {priceCatalog.FileId}.");

            //    throw new POSException($"product id - { priceCatalog.ProductId } not parseable in {priceCatalog.FileId}.");
            //}
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