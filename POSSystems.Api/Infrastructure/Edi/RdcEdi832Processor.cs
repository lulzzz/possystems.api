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
    public class RdcEdi832Processor : IEdiProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Edi832Job> _logger;

        public RdcEdi832Processor(IUnitOfWork unitOfWork, ILogger<Edi832Job> logger)
        {
            this._unitOfWork = unitOfWork;
            this._logger = logger;
        }

        public void Process(string[] downloadedFiles, string ediPath, string inPath, string processingPath, int supplierId, string fieldSeparator)
        {
            string processedPath = Path.Combine(ediPath, processingPath);

            if (!string.IsNullOrEmpty(processingPath) && !Directory.Exists(processedPath))
                Directory.CreateDirectory(processedPath);

            if (downloadedFiles == null)
                downloadedFiles = Directory.GetFiles(Path.Combine(ediPath, inPath));

            foreach (string file in downloadedFiles)
            {
                BatchFile batchFile = null;
                try
                {
                    //_unitOfWork.ToggleTracking();
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

                    var segments = new List<string>(eRxData.Split(new string[] { @"\" },
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
                    int segCount = segments.Count;
                    int totalRowCount = 0;
                    for (int i = 0; i < segCount; ++i)
                    {
                        try
                        {
                            var item = segments[i];
                            var fieldList = new List<string>(item.Split(fieldSeparator).ToList());
                            var segmentName = fieldList[0].Trim();
                            switch (segmentName)
                            {
                                case "LIN":
                                    {
                                        priceCatalog.ProductQualifier = fieldList[4].Trim();
                                        priceCatalog.UPCCode = fieldList[5].Trim();
                                        priceCatalog.ItemNo = fieldList[3].Trim();
                                        if (fieldList[4].Trim() == "UP")
                                        {
                                            priceCatalog.ScanCode = fieldList[5].Remove(0, 1).Trim();
                                        }
                                        else
                                        {
                                            priceCatalog.ScanCode = fieldList[5].Remove(5, 1).Trim();
                                        }
                                    }
                                    break;

                                case "PID":
                                    {
                                        if (fieldList[1].Trim() == "F")
                                            priceCatalog.ProductName = Regex.Replace(fieldList[5], @"\s+", " ", RegexOptions.Compiled)
                                                .Replace("\"", "", StringComparison.InvariantCultureIgnoreCase)
                                                .Replace(",", "", StringComparison.InvariantCultureIgnoreCase).Trim();
                                        priceCatalog.ProductDescription = Regex.Replace(fieldList[5], @"\s+", " ", RegexOptions.Compiled)
                                            .Replace("\"", "", StringComparison.InvariantCultureIgnoreCase)
                                            .Replace(",", "", StringComparison.InvariantCultureIgnoreCase).Trim();
                                    }

                                    break;

                                case "CTP":
                                    {
                                        if (fieldList[2].Trim() == "NET")
                                            if (!string.IsNullOrEmpty(fieldList[3].Trim()))
                                                priceCatalog.PurchasePrice = fieldList[3].Trim();

                                        if (!string.IsNullOrEmpty(fieldList[3].Trim()))
                                            if (fieldList[2].Trim() == "AWP")
                                                priceCatalog.SalesPrice = fieldList[3].Trim();
                                    }

                                    break;
                            }

                            if ((segmentName == "CTP") && (fieldList[2].Trim() == "RES"))
                            {
                                totalRowCount++;
                                _unitOfWork.PriceCatalogRepository.Add(priceCatalog);

                                priceCatalog = new PriceCatalog { FileId = batchFile.FileId, SupplierId = supplierId };
                            }
                        }
                        catch
                        {
                            _logger.LogError($"Was not able to log error for {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                        }
                    }

                    if (!_unitOfWork.Save())
                    {
                        _logger.LogError($"Price catalogs inserting error.");
                        errorCount = totalRowCount;
                        //_unitOfWork.ToggleTracking();
                    }
                    else
                    {
                        _logger.LogInformation($"Price catalogs inserted in the database.");

                        //_unitOfWork.ToggleTracking();

                        var priceCatalogs = _unitOfWork.PriceCatalogRepository.Find(pr => pr.FileId == batchFile.FileId).ToList();

                        priceCatalog.Category = "OT";

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

                        priceCatalog.Unit = "Miscellaneous";
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

                        _unitOfWork.ToggleTimeout(10000);
                        _unitOfWork.ProductRepository.MergeProducts(productCategory.CategoryID, measurementUnit.MeasurementId, supplierId, false);
                        _unitOfWork.ToggleTimeout();

                        errorCount = _unitOfWork.PriceCatalogRepository.Find(pr => pr.Imported == false && pr.FileId == batchFile.FileId).Count();
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
                    _unitOfWork.ToggleTimeout();
                    _logger.LogError($"Status of file {batchFile.FileName} failed to update.");
                }
                catch (Exception ex)
                {
                    _unitOfWork.ToggleTimeout();
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

        private void InsertFromPriceCatalog(PriceCatalog priceCatalog, int measurementId)
        {
            string catalogProductId = priceCatalog.ItemNo;
            //if (int.TryParse(priceCatalog.ItemNo, out catalogProductId))
            //{
            var product = _unitOfWork.ProductRepository.GetProductByCatalog(catalogProductId, false);

            var pRealExists = _unitOfWork.ProductRepository.ExistsRealUpc(priceCatalog.UPCCode);
            var pScanExists = _unitOfWork.ProductRepository.ExistsScanUpc(priceCatalog.ScanCode);

            if (pRealExists == false && pScanExists == false)
            {
                var selectedPrice = _unitOfWork.PriceRangeRepository.SelectPrice(product, priceCatalog);

                int? nPackageSize = Helpers.StringExtensions.ReturnNullableInt(priceCatalog.PackageSize);
                double? nPurchasePrice = Helpers.StringExtensions.ReturnNullableDouble(priceCatalog.PurchasePrice);

                var productDetail = new Product
                {
                    UpcCode = priceCatalog.UPCCode,
                    UpcScanCode = priceCatalog.ScanCode,
                    SupplierId = priceCatalog.SupplierId,
                    MeasurementId = measurementId,
                    PackageSize = nPackageSize,
                    ProductId = product.ProductId,
                    PurchasePrice = nPurchasePrice,
                    Strength = priceCatalog.Strength,
                    SalesPrice = selectedPrice,
                    ItemNo = priceCatalog.ItemNo
                };

                _unitOfWork.ProductRepository.Add(productDetail);

                priceCatalog.Imported = true;
                if (!_unitOfWork.Save())
                {
                    _logger.LogInformation($"Unable to update price catalog {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                }
            }
            else if (pRealExists == true && pScanExists == true)
            {
                var productDetail = _unitOfWork.ProductRepository.GetByRealUpc(priceCatalog.UPCCode);

                var selectedPrice = _unitOfWork.PriceRangeRepository.SelectPrice(product, priceCatalog);
                double? nPurchasePrice = Helpers.StringExtensions.ReturnNullableDouble(priceCatalog.PurchasePrice);

                productDetail.PurchasePrice = nPurchasePrice;
                productDetail.SalesPrice = selectedPrice;

                priceCatalog.Imported = true;
                if (!_unitOfWork.Save())
                {
                    _logger.LogInformation($"Unable to update price catalog {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                }
            }
            else
            {
                priceCatalog.Error = "Ignored";
                if (!_unitOfWork.Save())
                {
                    _logger.LogInformation($"Price update for {priceCatalog.UPCCode} ignored.");
                }
            }
            //}
            //else
            //{
            //    _logger.LogError($"product id - { priceCatalog.ProductId } not parseable in {priceCatalog.FileId}.");

            //    throw new POSException($"product id - { priceCatalog.ProductId } not parseable in {priceCatalog.FileId}.");
            //}
        }

        public static string CategoryDescriptions()
        {
            string returnValue = null;

            return returnValue;
        }
    }
}