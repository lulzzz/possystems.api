using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSSystems.Core;
using POSSystems.Core.Dtos.Product;
using POSSystems.Core.Dtos.Report;
using POSSystems.Core.Dtos.Sales;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace POSSystems.Web.Controllers
{
    [Route("api/inventoryreport")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Report })]
    public class InventoryReportController : BaseController<InventoryReportController>
    {
        private readonly PrintingInfo _printingInfo;

        public InventoryReportController(IUnitOfWork unitOfWork,
            ILogger<InventoryReportController> logger,
            IOptions<PrintingInfo> printingInfoAccessor,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper) => _printingInfo = printingInfoAccessor.Value;

        [HttpGet("file", Name = "GetInventoryReportFile")]
        public IActionResult GetReportFile(string productname, string upcscancode, int? categoryId = null, int? supplierId = null, bool isPdf = false)
        {
            var productStockDtos = _unitOfWork.ReportRepository.GetProductsStock(upcscancode, productname, categoryId, supplierId);

            if (!productStockDtos.Any())
                return NotFound();

            if (isPdf)
            {
                var columnsHeader = new List<string>{
                    "Product Name",
                    "UPC Code",
                    "Quantity",
                    "AWP Cost",
                    "ACQ Cost"
                };

                var filecontent = ExportPDF(productStockDtos.ToList(), columnsHeader, "Inventory Report");
                return File(filecontent, "application/pdf", "InventoryReport " + DateTime.Today.ToShortDateString() + ".pdf"); ;
            }
            else
                return ExportSalesReport(productStockDtos.ToList());
        }

        private FileStreamResult ExportSalesReport(List<ProductStockDto> _smapleData)
        {
            var companyName = _printingInfo.CompanyName;
            var memoryStream = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Sheet 1"
                });
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var headerrow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                var header2row = new DocumentFormat.OpenXml.Spreadsheet.Row();

                headerrow.Append(
                     new DocumentFormat.OpenXml.Spreadsheet.Cell()
                     {
                         CellValue = new CellValue(companyName),
                         DataType = CellValues.String
                     },

                     new DocumentFormat.OpenXml.Spreadsheet.Cell()
                     {
                         CellValue = new CellValue("Inventory Report"),
                         DataType = CellValues.String
                     }
                );

                sheetData.AppendChild(headerrow);
                sheetData.AppendChild(header2row);

                var row = new DocumentFormat.OpenXml.Spreadsheet.Row();

                row.Append(
                      new DocumentFormat.OpenXml.Spreadsheet.Cell()
                      {
                          CellValue = new CellValue("UPC"),
                          DataType = CellValues.String
                      },
                      new DocumentFormat.OpenXml.Spreadsheet.Cell()
                      {
                          CellValue = new CellValue("Name"),
                          DataType = CellValues.String
                      },
                      new DocumentFormat.OpenXml.Spreadsheet.Cell()
                      {
                          CellValue = new CellValue("Quantity"),
                          DataType = CellValues.String
                      },
                      new DocumentFormat.OpenXml.Spreadsheet.Cell()
                      {
                          CellValue = new CellValue("AWP"),
                          DataType = CellValues.String
                      },
                      new DocumentFormat.OpenXml.Spreadsheet.Cell()
                      {
                          CellValue = new CellValue("ACQ"),
                          DataType = CellValues.String
                      }
                );

                sheetData.AppendChild(row);
                for (var i = 0; i < _smapleData.Count; i++)
                {
                    var data = _smapleData[i];
                    row = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    row.Append(
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.UpcCode.ToString()),
                            DataType = CellValues.String
                        },
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.ProductName ?? "-"),
                            DataType = CellValues.Date
                        },
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.Quantity != null ? data.Quantity.ToString() : "-"),
                            DataType = CellValues.String
                        },
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.PurchasePrice != null ? data.PurchasePrice.ToString() : "-"),
                            DataType = CellValues.Number
                        },
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.SalesPrice.ToString()),
                            DataType = CellValues.Number
                        }
                    );

                    sheetData.AppendChild(row);
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(memoryStream,
                "application/ms-excel");
        }

        private byte[] ExportPDF(List<ProductStockDto> dataList, List<string> columnsHeader, string heading)
        {
            var document = new Document(new Rectangle(612, 792));
            var outputMS = new MemoryStream();
            PdfWriter.GetInstance(document, outputMS);
            document.Open();
            var font5 = FontFactory.GetFont(FontFactory.HELVETICA, 9);

            var companyName = _printingInfo.CompanyName;

            document.Add(new Phrase(Environment.NewLine));
            iTextSharp.text.Font titleFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font regularFont = FontFactory.GetFont("Arial", 12);
            Paragraph title;
            Paragraph text;

            title = new Paragraph(companyName, titleFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(title);

            title = new Paragraph(heading)
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(title);

            text = new Paragraph("  ", regularFont);
            document.Add(text);

            var count = columnsHeader.Count;
            var table = new PdfPTable(count);
            float[] widths = new float[] { 21f, 5f, 4f, 4f, 4f };

            table.WidthPercentage = 100;
            table.SetWidths(widths);
            table.LockedWidth = true;

            for (int i = 0; i < count; i++)
            {
                var headerCell = new PdfPCell(new Phrase(columnsHeader[i], font5));
                if (i > 1)
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;

                headerCell.BackgroundColor = BaseColor.Gray;
                table.AddCell(headerCell);
            }

            foreach (var item in dataList)
            {
                var celal = new PdfPCell(new Phrase(item.ProductName.Trim(), font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.UpcCode, font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.Quantity.Value.ToString(), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.PurchasePrice.HasValue ? item.PurchasePrice.Value.ToString("F2") : "-", font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.SalesPrice.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);
            }

            table.LockedWidth = false;

            document.Add(table);
            document.Close();
            var result = outputMS.ToArray();

            return result;
        }
    }
}