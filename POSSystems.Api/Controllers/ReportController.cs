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
using POSSystems.Core.Dtos.Sales;
using POSSystems.Core.Dtos.SalesMaster;
using POSSystems.Core.Models;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace POSSystems.Web.Controllers
{
    [Route("api/dailysalesreport")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Report })]
    public class ReportController : BaseController<ReportController>
    {
        private readonly PrintingInfo _printingInfo;

        public ReportController(IUnitOfWork unitOfWork,
            ILogger<ReportController> logger,
            IOptions<PrintingInfo> printingInfoAccessor,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper) => _printingInfo = printingInfoAccessor.Value;

        [HttpGet("file", Name = "GetReportFile")]
        public IActionResult GetReportFile(bool isPdf = false, DateTime? startDate = null, DateTime? endDate = null, int? terminalId = null, int? userId = null, int? supplierId = null)
        {
            var terminalName = "All";
            if (terminalId != null)
            {
                var PosTerminal = _unitOfWork.PosTerminalRepository.Get(terminalId.Value);

                if (PosTerminal != null)
                {
                    terminalName = PosTerminal.TerminalName;
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

            endDate = endDate.Value.Add(new TimeSpan(0, 23, 59, 59));

            var salesMasterDtos = _unitOfWork.ReportRepository.GetAllSales(startDate.Value, endDate.Value, terminalId, userName, supplierId).ToList();

            if (isPdf)
            {
                //column Header name
                var columnsHeader = new List<string>{
                    "Invoice #",
                    "Sales Date",
                    //"Transaction Type",
                    "Total Charged",
                    "Tax",
                    "Discount",
                    "Tendered",
                    "Balance",
                    "Return"
                };

                var filecontent = ExportPDF(salesMasterDtos.ToList(), columnsHeader, "Sales Report", startDate.Value, endDate.Value, terminalName, userName);
                return File(filecontent, "application/pdf", "SalesReport " + startDate.Value + " " + endDate.Value + ".pdf"); ;
            }
            else
                return ExportSalesReport(salesMasterDtos.ToList(), startDate.Value, endDate.Value, terminalName, userName);
        }

        private FileStreamResult ExportSalesReport(List<SalesMasterDto> _smapleData, DateTime startDate, DateTime endDate, string terminalName, string userName)
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
                         CellValue = new CellValue("Sales Report (Terminal: " + terminalName + ",User: " + userName + ")"),
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

                var row = new DocumentFormat.OpenXml.Spreadsheet.Row();

                row.Append(
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Invoice No."),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Sales Date"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Transaction Type"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Total Charged"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Tax"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Discount"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Tendered"),
                        DataType = CellValues.String
                    }
                    ,
                      new DocumentFormat.OpenXml.Spreadsheet.Cell()
                      {
                          CellValue = new CellValue("Balance"),
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
                             // CellValue = new CellValue((i + 1).ToString()),
                             CellValue = new CellValue(data.InvoiceNo.ToString()),

                             DataType = CellValues.String
                         },
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.SalesDate != null ? data.SalesDate.Value.ToShortDateString() : "-"),
                            DataType = CellValues.Date
                        },
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.PayMethod != null ? data.PayMethod.ToString() : "-"),
                            DataType = CellValues.String
                        },

                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.GrandTotal != null ? data.GrandTotal.ToString() : "-"),
                            DataType = CellValues.Number
                        },

                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.SalesTax != null ? data.SalesTax.ToString() : "-"),
                            DataType = CellValues.Number
                        }, new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.TotalDiscount != null ? data.TotalDiscount.ToString() : "-"),
                            DataType = CellValues.Number
                        }, new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.Payment != null ? data.Payment.ToString() : "-"),
                            DataType = CellValues.Number
                        }
                        , new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.Due != null ? data.Due.ToString() : "-"),
                            DataType = CellValues.Number
                        }

                    );
                    sheetData.AppendChild(row);
                }
                var grandTotal = _smapleData.Sum(s => s.GrandTotal);
                var salesTax = _smapleData.Sum(s => s.SalesTax);
                var payment = _smapleData.Sum(s => s.Payment);
                var totalDiscount = _smapleData.Sum(s => s.TotalDiscount);
                var due = _smapleData.Sum(s => s.Due);

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
                        CellValue = new CellValue(grandTotal.ToString()),
                        DataType = CellValues.String
                    },

                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(salesTax.ToString()),
                        DataType = CellValues.String
                    }, new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(totalDiscount.ToString()),
                        DataType = CellValues.String
                    }, new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(payment.ToString()),
                        DataType = CellValues.String
                    }, new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(due.ToString()),
                        DataType = CellValues.String
                    });

                sheetData.AppendChild(footerrow);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(memoryStream,
                "application/ms-excel");
        }

        private byte[] ExportPDF(List<SalesMasterDto> dataList, List<string> columnsHeader, string heading, DateTime startDate, DateTime endDate, string terminalName, string userName)
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

            text = new Paragraph("Terminal: " + terminalName + ", User:" + userName, regularFont);
            document.Add(text);
            text = new Paragraph("Date:" + startDate + " - " + endDate, regularFont);
            document.Add(text);
            text = new Paragraph("  ", regularFont);
            document.Add(text);

            var count = columnsHeader.Count;
            var table = new PdfPTable(count);
            float[] widths = new float[] { 3f, 4f, 5f, 5f, 5f, 5f, 5f, 5f };

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
                if (i > 2)
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;

                headerCell.BackgroundColor = BaseColor.Gray;
                table.AddCell(headerCell);
            }

            var celal = new PdfPCell();
            foreach (var item in dataList)
            {
                celal = new PdfPCell(new Phrase(item.InvoiceNo, font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.SalesDate.Value.ToString("MM-dd-yyyy"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(celal);

                //celal = new PdfPCell(new Phrase(item.PayMethod, font5));
                //celal.HorizontalAlignment = Element.ALIGN_LEFT;
                //table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.GrandTotal.HasValue ? item.GrandTotal?.ToString("F2") : 0.00.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.SalesTax.HasValue ? item.SalesTax.Value.ToString("F2") : 0.00.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.TotalDiscount.HasValue ? item.TotalDiscount.Value.ToString("F2") : 0.00.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.Payment.HasValue ? item.Payment.Value.ToString("F2") : 0.00.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.Due.HasValue ? Math.Round(item.Due.Value, 2).ToString("F2") : 0.00.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);

                celal = new PdfPCell(new Phrase(item.ReturnAmount.HasValue ? Math.Round(item.ReturnAmount.Value, 2).ToString("F2") : 0.00.ToString("F2"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                table.AddCell(celal);
            }

            var grandTotal = dataList.Sum(s => s.GrandTotal);
            var salesTax = dataList.Sum(s => s.SalesTax);
            var payment = dataList.Sum(s => s.Payment);
            var totalDiscount = dataList.Sum(s => s.TotalDiscount);
            //var due = dataList.Sum(s => s.Due);
            var due = grandTotal - payment;
            var returnAmount = dataList.Sum(s => s.ReturnAmount);

            table.AddCell(new Phrase("", font5));
            table.AddCell(new Phrase("Grand Total", font5));

            celal = new PdfPCell(new Phrase(grandTotal?.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            table.AddCell(celal);

            celal = new PdfPCell(new Phrase(salesTax?.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            table.AddCell(celal);

            celal = new PdfPCell(new Phrase(totalDiscount?.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            table.AddCell(celal);

            celal = new PdfPCell(new Phrase(payment?.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            table.AddCell(celal);

            celal = new PdfPCell(new Phrase(Math.Round((decimal)due, 2).ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            table.AddCell(celal);

            celal = new PdfPCell(new Phrase(Math.Round((decimal)returnAmount, 2).ToString("F2"), font5))
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
    }
}