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
    public class CompleteCsvProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductController> _logger;
        private readonly IMapper _mapper;
        private readonly string _username;

        public CompleteCsvProcessor(IUnitOfWork unitOfWork, ILogger<ProductController> logger, IMapper mapper, string username)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _username = username;
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
                    batchFile = new BatchFile
                    {
                        FileName = Path.GetFileName(file),
                        Status = FileStatus.Started.Humanize(),
                        SupplierId = supplierId,
                        CreateDate = DateTime.Now
                    };

                    _unitOfWork.BatchFileRepository.Add(batchFile);
                    _unitOfWork.Save();

                    processedFile = MoveFile(processedPath, file);

                    using (TextReader reader = File.OpenText(processedFile))
                    {
                        // now initialize the CsvReader
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture);
                        using var csv = new CsvReader(reader: reader, configuration: config);
                        csv.Context.RegisterClassMap<ProductCSVMapping2>();

                        var records = csv.EnumerateRecords(new ProductCSVDto2());

                        foreach (var r in records)
                        {
                            var priceCatalog = new PriceCatalog { FileId = batchFile.FileId, SupplierId = supplierId };

                            try
                            {
                                _mapper.Map(r, priceCatalog);
                                _unitOfWork.PriceCatalogRepository.Add(priceCatalog);
                            }
                            catch
                            {
                                _logger.LogError($"Was not able to log error for {priceCatalog.UPCCode} in {priceCatalog.FileId}.");
                            }
                        }
                    }

                    if (!_unitOfWork.Save())
                    {
                        _logger.LogError($"Price catalogs inserting error. {batchFile?.FileName}");
                        batchFile.Status = FileStatus.Failed.Humanize();
                    }
                    else
                    {
                        _logger.LogInformation($"Price catalogs inserted in the database.");

                        //_unitOfWork.ToggleTracking();

                        var priceCatalogs = _unitOfWork.PriceCatalogRepository.Find(pr => pr.FileId == batchFile.FileId).ToList();

                        var categories = priceCatalogs.GroupBy(pc => pc.Category)
                            .Select(grp => grp.First())
                            .Where(c => c.FileId == batchFile.FileId && c.Category != null)
                            .ToList();

                        foreach (var category in categories)
                        {
                            var productCategory = _unitOfWork.ProductCategoryRepository.Get(category.Category);
                            if (productCategory == null)
                            {
                                productCategory = new ProductCategory { CategoryName = category.Category };
                                _unitOfWork.ProductCategoryRepository.Add(productCategory);
                            }
                        }

                        var measurementUnits = priceCatalogs.GroupBy(pc => pc.Unit)
                            .Select(grp => grp.First())
                            .Where(c => c.FileId == batchFile.FileId && c.Unit != null)
                            .ToList();

                        foreach (var unit in measurementUnits)
                        {
                            var measurementUnit = _unitOfWork.MeasurementUnitRepository.GetByName(unit.Unit);
                            if (measurementUnit == null)
                            {
                                measurementUnit = new MeasurementUnit { MeasurementName = unit.Unit };
                                _unitOfWork.MeasurementUnitRepository.Add(measurementUnit);
                            }
                        }

                        var manufacturers = priceCatalogs.GroupBy(pc => pc.Manufacturer)
                            .Select(grp => grp.First())
                            .Where(c => c.FileId == batchFile.FileId && c.Manufacturer != null)
                            .ToList();

                        foreach (var priceCatalogManufacturer in manufacturers)
                        {
                            var manufacturer = _unitOfWork.ManufacturerRepository.GetByName(priceCatalogManufacturer.Manufacturer);
                            if (manufacturer == null)
                            {
                                manufacturer = new Manufacturer { Name = priceCatalogManufacturer.Manufacturer };
                                _unitOfWork.ManufacturerRepository.Add(manufacturer);
                            }
                        }

                        if (!_unitOfWork.Save())
                        {
                            throw new POSException("Unable to insert parent data.");
                        }

                        _unitOfWork.ToggleTimeout(10000);
                        _unitOfWork.ProductRepository.MergeProducts(batchFile.FileId, supplierId, true, _username);
                        _unitOfWork.ToggleTimeout();

                        errorCount = _unitOfWork.PriceCatalogRepository.Find(pr => pr.Imported == false && pr.FileId == batchFile.FileId).Count();
                    }

                    if (errorCount == 0 && batchFile.Status != FileStatus.Failed.Humanize())
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
                }
                catch (LogException)
                {
                    _unitOfWork.ToggleTimeout();
                    _logger.LogError($"Status of file {batchFile?.FileName} failed to update.");
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

        private string MoveFile(string processedPath, string file)
        {
            string processedFile = Path.Combine(processedPath, Path.GetFileName(file));
            if (File.Exists(processedFile))
                File.Delete(processedFile);

            File.Move(file, processedFile);
            return processedFile;
        }
    }
}