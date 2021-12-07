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
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace POSSystems.Web.Controllers
{
    [Route("api/timesheetreport")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Report })]
    public class TimesheetReportController : BaseController<TimesheetReportController>
    {
        private readonly PrintingInfo _printingInfo;

        public TimesheetReportController(IUnitOfWork unitOfWork,
             ILogger<TimesheetReportController> logger,
             IOptions<PrintingInfo> printingInfoAccessor,
             IUrlHelper urlHelper,
             IPropertyMappingService propertyMappingService,
             IMapper mapper)
             : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper) => _printingInfo = printingInfoAccessor.Value;

        [HttpGet("file", Name = "GetTimesheetReportFile")]
        public IActionResult GetTimesheetReportFile(bool isPdf = false, DateTime? startTime = null, DateTime? endTime = null, int? userId = null)
        {
            var username = "All";
            if (userId != null)
            {
                var user = _unitOfWork.UserRepository.Get(userId.Value);

                if (user != null)
                {
                    username = user.UserName;
                }
            }

            if (!startTime.HasValue)
            {
                startTime = DateTime.Now.Date;
            }

            if (!endTime.HasValue)
            {
                endTime = DateTime.Now;
            }

            if (startTime > endTime) return StatusCode(400, "Start time cannot be greater than end time.");

            var timesheetDtos = _unitOfWork.ReportRepository.GetTimesheet(startTime.Value, endTime.Value, userId).ToList();

            if (isPdf)
            {
                var columnsHeader = new List<string>{
                    "Username",
                    "Start time",
                    "End Time",
                    "Total hours"
                };

                var filecontent = ExportPDF(timesheetDtos.ToList(), columnsHeader, "Timesheet Report", startTime.Value, endTime.Value, username);
                return File(filecontent, "application/pdf", "TimesheetReport " + startTime.Value + " " + endTime.Value + ".pdf"); ;
            }
            else
                return ExportSalesReport(timesheetDtos.ToList(), startTime.Value, endTime.Value, username);
        }

        private byte[] ExportPDF(List<TimesheetDto> dataList, List<string> columnsHeader, string heading, DateTime startDate, DateTime endDate, string username)
        {
            var companyName = _printingInfo.CompanyName;

            var document = new Document(new Rectangle(792, 612));
            var outputMS = new MemoryStream();
            PdfWriter.GetInstance(document, outputMS);
            document.Open();
            var font5 = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            document.Add(new Phrase(Environment.NewLine));
            iTextSharp.text.Font titleFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font regularFont = FontFactory.GetFont("Arial", 12);

            document.Add(new Paragraph(companyName, titleFont)
            {
                Alignment = Element.ALIGN_CENTER
            });

            document.Add(new Paragraph(heading)
            {
                Alignment = Element.ALIGN_CENTER
            });

            document.Add(new Paragraph("User: " + username, regularFont));
            document.Add(new Paragraph("Date:" + startDate + " - " + endDate, regularFont));
            document.Add(new Paragraph("  ", regularFont));

            var count = columnsHeader.Count;
            var table = new PdfPTable(count);
            float[] widths = new float[] { 4f, 4f, 4f, 4f };

            table.WidthPercentage = 100;
            table.SetWidths(widths);
            table.LockedWidth = true;

            for (int i = 0; i < count; i++)
            {
                var headerCell = new PdfPCell(new Phrase(columnsHeader[i], font5));
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell.BackgroundColor = BaseColor.Gray;
                table.AddCell(headerCell);
            }

            foreach (var item in dataList)
            {
                var cell = new PdfPCell(new Phrase(item.UserName, font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(item.StartTime.ToString("g"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(item.EndTime?.ToString("g"), font5))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(item.TimeDifferenceStr, font5))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                table.AddCell(cell);
            }

            var grandTimeSpent = dataList.Sum(s => s.TimeDifference.TotalSeconds);

            table.AddCell(new Phrase(" ", font5));
            table.AddCell(new Phrase(" ", font5));
            table.AddCell(new Phrase("Total Hours:", font5));

            var totalCell = new PdfPCell(new Phrase(TimeHelper.TotalHoursFromSeconds(grandTimeSpent), font5))
            {
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            table.AddCell(totalCell);

            table.LockedWidth = false;

            document.Add(table);
            document.Close();

            var result = outputMS.ToArray();

            return result;
        }

        private FileStreamResult ExportSalesReport(List<TimesheetDto> timesheetDtos, DateTime startDate, DateTime endDate, string username)
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
                         CellValue = new CellValue("Timesheet Report (User: " + username + ")"),
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
                        CellValue = new CellValue("Username"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Start time"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("End Time"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Total Time"),
                        DataType = CellValues.String
                    }
                );

                sheetData.AppendChild(row);
                for (var i = 0; i < timesheetDtos.Count; i++)
                {
                    var data = timesheetDtos[i];
                    row = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    row.Append(
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.UserName),
                            DataType = CellValues.String
                        },
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.StartTime.ToString("g")),
                            DataType = CellValues.String
                        },
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.EndTime?.ToString("g")),
                            DataType = CellValues.String
                        },
                        new DocumentFormat.OpenXml.Spreadsheet.Cell()
                        {
                            CellValue = new CellValue(data.TimeDifferenceStr),
                            DataType = CellValues.String
                        }
                    );

                    sheetData.AppendChild(row);
                }

                var grandTimeSpent = timesheetDtos.Sum(s => s.TimeDifference.TotalSeconds);

                var footerrow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                footerrow.Append(
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue("Total Hours"),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(string.Empty),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(string.Empty),
                        DataType = CellValues.String
                    },
                    new DocumentFormat.OpenXml.Spreadsheet.Cell()
                    {
                        CellValue = new CellValue(TimeHelper.TotalHoursFromSeconds(grandTimeSpent)),
                        DataType = CellValues.String
                    });

                sheetData.AppendChild(footerrow);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(memoryStream, "application/ms-excel");
        }
    }
}