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
using System.IO;

namespace POSSystems.Web.Controllers
{
    [Route("api/salesendreport")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Report })]
    public class SalesEndReportController : BaseController<SalesEndReportController>
    {
        private readonly PrintingInfo _printingInfo;

        public SalesEndReportController(IUnitOfWork unitOfWork,
            ILogger<SalesEndReportController> logger,
            IOptions<PrintingInfo> printingInfoAccessor,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper) => _printingInfo = printingInfoAccessor.Value;

        [HttpGet("file", Name = "GetSalesEndReportFile")]
        public IActionResult GetSalesEndReportFile(int? userId, DateTime? startDate = null, bool isPdf = false)
        {
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

            var salesEndDto = _unitOfWork.ReportRepository.GetSalesEnd(startDate.Value, userName);

            if (isPdf)
            {
                var filecontent = ExportPDF(salesEndDto, "Sales End Report", startDate.Value, userName);
                return File(filecontent, "application/pdf", "SalesEndReport " + startDate.Value + ".pdf"); ;
            }
            else
                return ExportSalesReport(salesEndDto, startDate.Value, userName);
        }

        private FileStreamResult ExportSalesReport(SalesEndDto salesEndDto, DateTime startDate, string userName)
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
                         CellValue = new CellValue("Sales End Report (User: " + userName + ")"),
                         DataType = CellValues.String
                     }
                );
                header3row.Append(

                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Date: " + startDate.ToShortDateString()),
                        DataType = CellValues.String
                    }

                );

                sheetData.AppendChild(headerrow);
                sheetData.AppendChild(header2row);

                var total = salesEndDto.Total;
                var totalTax = salesEndDto.TotalTax;

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
                        CellValue = new CellValue(total.ToString()),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(totalTax.ToString()),
                        DataType = CellValues.String
                    }
                );

                sheetData.AppendChild(footerrow);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(memoryStream,
                "application/ms-excel");
        }

        private byte[] ExportPDF(SalesEndDto salesEndDto, string heading, DateTime startDate, string userName)
        {
            var companyName = _printingInfo.CompanyName;

            var document = new Document(new Rectangle(612, 792));
            var outputMS = new MemoryStream();
            PdfWriter.GetInstance(document, outputMS);
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

            title = new Paragraph(heading + "(" + startDate.ToShortDateString() + ")")
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(title);

            text = new Paragraph("User:" + userName, regularFont);
            document.Add(text);
            text = new Paragraph("  ", regularFont);
            document.Add(text);

            var tableTotal = new PdfPTable(2)
            {
                WidthPercentage = 50
            };

            float[] widths = new float[] { 15f, 6f };
            tableTotal.SetWidths(widths);

            tableTotal.AddCell(new Phrase("Total", font5));
            var celal = new PdfPCell(new Phrase(salesEndDto.Total.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableTotal.AddCell(celal);

            tableTotal.AddCell(new Phrase("Total tax", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.TotalTax.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableTotal.AddCell(celal);

            tableTotal.AddCell(new Phrase("Total points earned", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.TotalPointsEarned.ToString(), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableTotal.AddCell(celal);

            tableTotal.AddCell(new Phrase("Total points redeemed", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.TotalPointsRedeemed.ToString(), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableTotal.AddCell(celal);

            tableTotal.AddCell(new Phrase("Total discount", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.TotalDiscount.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableTotal.AddCell(celal);

            tableTotal.AddCell(new Phrase("Total customer", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.TotalCustomer.ToString(), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableTotal.AddCell(celal);

            document.Add(tableTotal);
            document.Add(new Phrase(Environment.NewLine));

            var tableSales = new PdfPTable(2)
            {
                WidthPercentage = 50
            };
            tableSales.SetWidths(widths);

            tableSales.AddCell(new Phrase("Cash Tendered", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.Sales.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableSales.AddCell(celal);

            tableSales.AddCell(new Phrase("Cash Balance", font5));
            celal = new PdfPCell(new Phrase((salesEndDto.Total - salesEndDto.Sales).ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableSales.AddCell(celal);

            tableSales.AddCell(new Phrase("Return", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.Return.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableSales.AddCell(celal);

            document.Add(tableSales);
            document.Add(new Phrase(Environment.NewLine));

            var tableMethod = new PdfPTable(2)
            {
                WidthPercentage = 50
            };
            tableMethod.SetWidths(widths);

            tableMethod.AddCell(new Phrase("Bank", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.Bank.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableMethod.AddCell(celal);

            tableMethod.AddCell(new Phrase("Check", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.Check.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableMethod.AddCell(celal);

            document.Add(tableMethod);

            var tableCard = new PdfPTable(2)
            {
                WidthPercentage = 50
            };
            tableCard.SetWidths(widths);

            tableCard.AddCell(new Phrase("Credit", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.Card.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableCard.AddCell(celal);

            tableCard.AddCell(new Phrase("Debit", font5));
            celal = new PdfPCell(new Phrase(salesEndDto.Debit.ToString("F2"), font5))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            tableCard.AddCell(celal);

            document.Add(tableCard);

            if (salesEndDto.CategoryPrices.Count > 0)
            {
                document.Add(new Phrase(Environment.NewLine));

                var tableCategory = new PdfPTable(2)
                {
                    WidthPercentage = 50
                };
                tableCategory.SetWidths(widths);

                tableCategory.AddCell(new Phrase("Category Sales:", font5));
                tableCategory.AddCell(new Phrase("", font5));
                foreach (var cp in salesEndDto.CategoryPrices)
                {
                    tableCategory.AddCell(new Phrase(cp.K, font5));
                    celal = new PdfPCell(new Phrase(cp.V.ToString("F2"), font5))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };
                    tableCategory.AddCell(celal);
                }

                document.Add(tableCategory);
            }

            document.Close();
            var result = outputMS.ToArray();

            return result;
        }
    }
}