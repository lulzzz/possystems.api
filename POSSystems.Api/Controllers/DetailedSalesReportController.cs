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
    [Route("api/detailedsalesreport")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Report })]
    public class DetailedSalesReportController : BaseController<DetailedSalesReportController>
    {
        private readonly PrintingInfo _printingInfo;

        public DetailedSalesReportController(IUnitOfWork unitOfWork,
             ILogger<DetailedSalesReportController> logger,
             IOptions<PrintingInfo> printingInfoAccessor,
             IUrlHelper urlHelper,
             IPropertyMappingService propertyMappingService,
             IMapper mapper)
             : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper) => _printingInfo = printingInfoAccessor.Value;

        [HttpGet("file", Name = "GetDetailedSalesReportFile")]
        public IActionResult GetDetailedSales(bool isPdf = false, DateTime? startDate = null, DateTime? endDate = null, int? terminalId = null, int? userId = null, int? supplierId = null)
        {
            var terminalName = "All";
            if (terminalId != null)
            {
                var posTerminal = _unitOfWork.PosTerminalRepository.Get(terminalId.Value);

                if (posTerminal != null)
                {
                    terminalName = posTerminal.TerminalName;
                }
            }

            var userName = "All";
            if (userId != null)
            {
                var user = _unitOfWork.UserRepository.Get(userId.Value);

                if (user != null)
                {
                    userName = user.UserName;
                }
            }

            if (!startDate.HasValue)
            {
                startDate = DateTime.Now;
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.Now;
            }

            var detailedSalesDtos = _unitOfWork.ReportRepository.GetDetailedSales(startDate.Value, endDate.Value, terminalId, userName, supplierId).ToList();

            if (isPdf)
            {
                //column Header name
                var columnsHeader = new List<string>{
                    "Supplier",
                    "Product Name",
                    "UPC Code",
                    "Date Sold",
                    "Qty Sold",
                    "Sold Price",
                    "Discount",
                    "Total Sales"
                };

                var filecontent = ExportPDF(detailedSalesDtos.ToList(), columnsHeader, "Detailed Sales Report", startDate.Value, endDate.Value, terminalName, userName);
                return File(filecontent, "application/pdf", "DetailedSalesReport " + startDate.Value + " " + endDate.Value + ".pdf"); ;
            }
            else
                return ExportSalesReport(detailedSalesDtos.ToList(), startDate.Value, endDate.Value, terminalName, userName);
        }

        private byte[] ExportPDF(List<DetailedSalesDto> dataList, List<string> columnsHeader, string heading, DateTime startDate, DateTime endDate, string terminalName, string userName)
        {
            var companyName = _printingInfo.CompanyName;

            var document = new Document(new Rectangle(792, 612));
            var outputMS = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, outputMS);
            document.Open();
            var font5 = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            document.Add(new Phrase(Environment.NewLine));
            iTextSharp.text.Font titleFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font regularFont = FontFactory.GetFont("Arial", 12);
            Paragraph title, text;

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

            text = new Paragraph("Terminal: " + terminalName, regularFont);
            document.Add(text);
            text = new Paragraph("User: " + userName, regularFont);
            document.Add(text);
            text = new Paragraph("Date:" + startDate + " - " + endDate, regularFont);
            document.Add(text);
            text = new Paragraph("  ", regularFont);
            document.Add(text);

            var count = columnsHeader.Count;
            var table = new PdfPTable(count);
            float[] widths = new float[] { 4f, 12f, 4f, 3f, 3f, 3f, 3f, 3f };

            table.WidthPercentage = 100;
            table.SetWidths(widths);
            table.LockedWidth = true;

            var cell = new PdfPCell(new Phrase(heading))
            {
                Colspan = count
            };

            for (int i = 0; i < count; i++)
            {
                var headerCell = new PdfPCell(new Phrase(columnsHeader[i], font5));
                if (i > 3)
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;

                headerCell.BackgroundColor = BaseColor.Gray;
                table.AddCell(headerCell);
            }

            var celal = new PdfPCell();
            foreach (var item in dataList)
            {
                celal = new PdfPCell(new Phrase(item.SupplierName, font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.ProductName, font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.UPCCode, font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.SalesDate.Value.ToString("MM/dd/yyyy"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.Quantity?.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.Price?.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.Discount?.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.SalesPrice?.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);
            }

            var grandDiscount = dataList.Sum(s => s.Discount);
            var grandTotalSold = dataList.Sum(s => s.SalesPrice);

            table.AddCell(new Phrase(" ", font5));
            table.AddCell(new Phrase(" ", font5));
            table.AddCell(new Phrase(" ", font5));
            table.AddCell(new Phrase(" ", font5));
            table.AddCell(new Phrase(" ", font5));
            table.AddCell(new Phrase("Grand Total:", font5));

            celal = new PdfPCell(new Phrase(grandDiscount?.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            table.AddCell(celal);

            celal = new PdfPCell(new Phrase(grandTotalSold?.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            table.AddCell(celal);

            table.LockedWidth = false;

            document.Add(table);
            document.Close();

            var result = outputMS.ToArray();

            return result;
        }

        private FileStreamResult ExportSalesReport(List<DetailedSalesDto> _smapleData, DateTime startDate, DateTime endDate, string terminalName, string userName)
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
                var header3row = new DocumentFormat.OpenXml.Spreadsheet.Row();

                headerrow.Append(
                     new DocumentFormat.OpenXml.Spreadsheet.Cell()
                     {
                         CellValue = new CellValue(companyName),
                         DataType = CellValues.String
                     }
                );
                header2row.Append(
                     new DocumentFormat.OpenXml.Spreadsheet.Cell()
                     {
                         CellValue = new CellValue("Detailed Sales Report (Terminal: " + terminalName + ",User: " + userName + ")"),
                         DataType = CellValues.String
                     }
                );
                header3row.Append(

                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Date: " + startDate.ToShortDateString() + "  -  " + endDate.ToShortDateString()),
                        DataType = CellValues.String
                    }

               );
                sheetData.AppendChild(headerrow);
                sheetData.AppendChild(header2row);
                sheetData.AppendChild(header3row);

                var row = new DocumentFormat.OpenXml.Spreadsheet.Row();

                row.Append(
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Supplier Name"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Product Name"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("UPC Code"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Qty Sold"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Sold Priced"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Total Sales"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Total Discount"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Date Sold"),
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
                             CellValue = new CellValue(data.SupplierName),

                             DataType = CellValues.String
                         },

                         new DocumentFormat.OpenXml.Spreadsheet.Cell()
                         {
                             CellValue = new CellValue(data.ProductName),

                             DataType = CellValues.String
                         },

                         new DocumentFormat.OpenXml.Spreadsheet.Cell()
                         {
                             CellValue = new CellValue(data.UPCCode),

                             DataType = CellValues.String
                         },

                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.Quantity != null ? data.Quantity.ToString() : "-"),
                            DataType = CellValues.String
                        },

                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.Price != null ? data.Price.ToString() : "-"),
                            DataType = CellValues.Number
                        },

                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.Discount != null ? data.Discount.ToString() : "-"),
                            DataType = CellValues.Number
                        },

                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue((data.SalesPrice) != null ? (data.SalesPrice).ToString() : "-"),
                            DataType = CellValues.Number
                        },

                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.SalesDate != null ? data.SalesDate.Value.ToString("MM/dd/yyyy") : "-"),
                            DataType = CellValues.Number
                        }
                    );
                    sheetData.AppendChild(row);
                }

                var grandDiscount = _smapleData.Sum(s => s.Discount);
                var grandTotalSold = _smapleData.Sum(s => s.SalesPrice);

                var footerrow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                footerrow.Append(
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Total."),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(""),
                        DataType = CellValues.String
                    }, new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(""),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(grandDiscount.ToString()),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(grandTotalSold.ToString()),
                        DataType = CellValues.String
                    }
                    );

                sheetData.AppendChild(footerrow);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(memoryStream,
                "application/ms-excel");
        }
    }
}