using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Humanizer;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Product;
using POSSystems.Core.Exceptions;
using POSSystems.Core.Models;
using POSSystems.Web.Controllers;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace POSSystems.Web.Infrastructure.Edi
{
    public class CsvProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductController> _logger;
        private readonly IMapper _mapper;

        public CsvProcessor(IUnitOfWork unitOfWork, ILogger<ProductController> logger, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._logger = logger;
            this._mapper = mapper;
        }

        public void Process(string localPath, string inPath, string processingPath, int supplierId)
        {
            int errorCount = 0;
            string processedPath = Path.Combine(localPath, processingPath);
            string processedFile = string.Empty;

            if (!string.IsNullOrEmpty(processingPath) && !Directory.Exists(processedPath))
                Directory.CreateDirectory(processedPath);

            var csvFiles = Directory.GetFiles(Path.Combine(localPath, inPath));

            foreach (string file in csvFiles)
            {
                BatchFile batchFile = null;
                try
                {
                    //_unitOfWork.ToggleTracking();
                    batchFile = new BatchFile
                    {
                        FileName = Path.GetFileName(file),
                        Status = FileStatus.Started.Humanize(),
                        SupplierId = supplierId,
                        CreateDate = DateTime.Now
                    };

                    _unitOfWork.BatchFileRepository.Add(batchFile);
                    if (!_unitOfWork.Save())
                    {
                        throw new LogException();
                    }

                    if (!string.IsNullOrEmpty(processingPath))
                    {
                        processedFile = Path.Combine(processedPath, Path.GetFileName(file));
                        if (File.Exists(processedFile))
                            File.Delete(processedFile);

                        File.Move(file, processedFile);
                    }

                    using (TextReader reader = File.OpenText(processedFile))
                    {
                        var supplierName = reader.ReadLine().Replace(',', ' ').Trim();
                        reader.ReadLine();
                        reader.ReadLine();
                        reader.ReadLine();
                        reader.ReadLine();
                        // now initialize the CsvReader
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = false,
                            HeaderValidated = null
                        };
                        using var csv = new CsvReader(reader: reader, configuration: config);
                        csv.Context.RegisterClassMap<ProductCSVMapping>();

                        var supplier = _unitOfWork.SupplierRepository.SingleOrDefault(s => s.SupplierName == supplierName);
                        if (supplier == null)
                        {
                            supplier = new Supplier { SupplierName = supplierName };
                            _unitOfWork.SupplierRepository.Add(supplier);

                            if (!_unitOfWork.Save())
                            {
                                _logger.LogError($"Price catalogs inserting error.");
                                //errorCount = totalRowCount;
                                //_unitOfWork.ToggleTracking();
                            }
                            else
                                supplierId = supplier.SupplierId;
                        }
                        else
                            supplierId = supplier.SupplierId;

                        //csv.ReadHeader();
                        //var records = csv.GetRecords<ProductCSVDto>().ToList();
                        var record = new ProductCSVDto();
                        var records = csv.EnumerateRecords(record);
                        var priceCatalog = new PriceCatalog { FileId = batchFile.FileId, SupplierId = supplier.SupplierId };

                        foreach (var r in records)
                        {
                            try
                            {
                                _mapper.Map(r, priceCatalog);

                                _unitOfWork.PriceCatalogRepository.Add(priceCatalog);
                                priceCatalog = new PriceCatalog { FileId = batchFile.FileId, SupplierId = supplier.SupplierId };
                            }
                            catch
                            {
                                _logger.LogError($"Was not able to log error for {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                            }
                        }
                    }

                    if (!_unitOfWork.Save())
                    {
                        _logger.LogError($"Price catalogs inserting error.");
                        //errorCount = totalRowCount;
                        //_unitOfWork.ToggleTracking();
                    }
                    else
                    {
                        _logger.LogInformation($"Price catalogs inserted in the database.");

                        //_unitOfWork.ToggleTracking();

                        var priceCatalogs = _unitOfWork.PriceCatalogRepository.Find(pr => pr.FileId == batchFile.FileId).ToList();
                        //int processedRowsCount = 0;

                        var priceCatalog = new PriceCatalog { FileId = batchFile.FileId };

                        var categories = priceCatalogs.GroupBy(pc => pc.Category)
                            .Select(grp => grp.First())
                            .Where(c => c.FileId == priceCatalog.FileId)
                            .ToList();

                        foreach (var category in categories)
                        {
                            var productCategory = _unitOfWork.ProductCategoryRepository.Get(category.Category);
                            if (productCategory == null)
                            {
                                productCategory = new ProductCategory { CategoryName = category.Category };
                                _unitOfWork.ProductCategoryRepository.Add(productCategory);
                                if (!_unitOfWork.Save())
                                {
                                    throw new POSException($"Unable to insert category {category.Category}.");
                                }
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
                        _unitOfWork.ProductRepository.MergeProducts(priceCatalog.FileId, measurementUnit.MeasurementId, supplierId, true);
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
    }
}