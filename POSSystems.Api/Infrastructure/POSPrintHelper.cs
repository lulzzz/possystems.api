using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using POSSystems.Core;
using POSSystems.Core.Dtos.Sales;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

//using ZXing.Common;
//using static BarcodeGenerator;

namespace POSSystems.Infrastructure
{
    /// <summary>
    /// POS Print Helper
    /// </summary>
    public class POSPrintHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly bool _printOnlyRx;

        public string Notes { get; set; }

        public POSPrintHelper(IWebHostEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;

        public POSPrintHelper(IWebHostEnvironment hostingEnvironment, bool printOnlyRx)
        {
            _hostingEnvironment = hostingEnvironment;
            _printOnlyRx = printOnlyRx;
        }

        #region -- [ Docs ] --

        //public MemoryStream CreateNewWordDocument(InvoiceDTO invoiceList)
        //{
        //    //StringRenderer Renderer = new StringRenderer() { Block = " ", Empty = "\u2588\u2588", NewLine = "\n    ", };
        //    //EncodingOptions Options = new EncodingOptions { Height = 5, Width = 2, Margin = 1 , PureBarcode=true};

        //    //  var genText=  Generate(Renderer, invoiceList.InvoiceNo, Options,ZXing.BarcodeFormat.CODE_128);
        //    string prodCode = invoiceList?.InvoiceNo;

        //    var code128 = new Barcode128();
        //    code128.CodeType = Barcode.CODE128;
        //    code128.ChecksumText = true;
        //    code128.GenerateChecksum = true;
        //    code128.Code = prodCode;
        //    var bm = new Bitmap(code128.CreateDrawingImage(System.Drawing.Color.Black, System.Drawing.Color.White));
        //    var img = code128.CreateDrawingImage(System.Drawing.Color.Black, System.Drawing.Color.White);
        //    //strbase64= CreateBase64Image(  new byte[2], img);
        //    var tempbarcodesPath = _hostingEnvironment.ContentRootPath + "/tempbarcodes/";
        //    var exists = Directory.Exists(tempbarcodesPath);

        //    if (!exists)
        //        Directory.CreateDirectory(tempbarcodesPath);

        //    var imagePath = Path.Combine(tempbarcodesPath, Guid.NewGuid() + ".jpg");
        //    img.Save(imagePath, ImageFormat.Bmp);

        //    var stream = new MemoryStream();
        //    //using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(WebClientPri, WordprocessingDocumentType.Document))
        //    using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        //    {
        //        string altChunkId = "myId";
        //        string altChunkId2 = "myId2";
        //        // Set the content of the document so that Word can open it.
        //        var mainPart = wordDoc.AddMainDocumentPart();
        //        //proce template

        //        var str = GetProcessTemplate(Template, invoiceList);

        //        str = str.Replace("{ImgUrl}", imagePath, StringComparison.InvariantCultureIgnoreCase);
        //        // str = str + "<br/><br/>" + str;

        //        var ms = new MemoryStream(Encoding.UTF8.GetBytes(str));

        //        // Uncomment the following line to create an invalid word document.
        //        // MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("<h1>HELLO</h1>"));

        //        // Create alternative format import part.
        //        var formatImportPart =
        //           mainPart.AddAlternativeFormatImportPart(
        //              AlternativeFormatImportPartType.Html, altChunkId);

        //        //ms.Seek(0, SeekOrigin.Begin);

        //        // Feed HTML data into format import part (chunk).
        //        formatImportPart.FeedData(ms);
        //        var altChunk = new AltChunk();
        //        altChunk.Id = altChunkId;
        //        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        //        mainPart.Document.Body = new Body();
        //        mainPart.Document.Body.Append(altChunk);

        //        var PageBreakParagraph = new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Break() { Type = BreakValues.Page }));
        //        mainPart.Document.Body.Append(PageBreakParagraph);
        //        // 2nd page
        //        var ms2 = new MemoryStream(Encoding.UTF8.GetBytes("<br/><br/>" + str));
        //        var formatImportPart2 =
        //           mainPart.AddAlternativeFormatImportPart(
        //              AlternativeFormatImportPartType.Html, altChunkId2);
        //        formatImportPart2.FeedData(ms2);
        //        var altChunk2 = new AltChunk();
        //        altChunk2.Id = altChunkId2;
        //        mainPart.Document.Body.Append(altChunk2);

        //        var pageMargins = new PageMargin();
        //        pageMargins.Left = 10;
        //        pageMargins.Right = 10;
        //        pageMargins.Bottom = 10;
        //        pageMargins.Top = 10;
        //        // PageSize pageSize = new PageSize();
        //        // pageSize.Width = 2000;
        //        //pageMargins.Header = 1500; //not needed for now
        //        //pageMargins.Footer = 1500; //not needed for now

        //        //Section section = mainPart.Document.AddSection();
        //        //Important needed to access properties (sections) to set values for all elements.

        //        // SectionProperties SecPro = new SectionProperties();
        //        var pSize = new PageSize();
        //        pSize.Width = 7500;
        //        //PSize.Height = 11000;
        //        // SecPro.Append(PSize);
        //        //mainPart.Document.Append(SecPro);
        //        //mainPart.Document.Append(SecPro);
        //        //mainPart.Append(SecPro);
        //        var body = mainPart.Document.Body;
        //        var sectionProps = new SectionProperties();
        //        sectionProps.Append(pageMargins);
        //        //sectionProps.Append(PSize);
        //        // body.Append(SecPro);
        //        body.Append(sectionProps);
        //        // SetMainDocumentContent(mainPart);

        //        return stream;
        //    }
        //}

        //private string CreateBase64Image(Image streamImage)
        //{
        //    /* Ensure we've streamed the document out correctly before we commit to the conversion */
        //    //using (var ms = new MemoryStream(fileBytes))
        //    //{
        //    /* Create a new image, saved as a scaled version of the original */
        //    //streamImage = ScaleImage(Image.FromStream(ms));
        //    //}
        //    using (var ms = new MemoryStream())
        //    {
        //        /* Convert this image back to a base64 string */
        //        streamImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        //        return Convert.ToBase64String(ms.ToArray());
        //    }
        //}

        //// Set content of MainDocumentPart.
        //public void SetMainDocumentContent(MainDocumentPart part, string sampleXml)
        //{
        //    //    const string docXml =
        //    //        @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
        //    //<w:document xmlns:w=""http://schemas.openxmlformats.org /wordprocessingml/2006/main"">
        //    //<w:body><w:p><w:r><w:t>Hello world!</w:t></w:r></w:p></w:body>
        //    // </w:document>";

        //    using (Stream stream = part.GetStream())
        //    {
        //        byte[] buf = (new UTF8Encoding()).GetBytes(sampleXml);
        //        stream.Write(buf, 0, buf.Length);
        //    }
        //}

        //public string GetProcessTemplate(string template, InvoiceDTO invoiceDTO)
        //{
        //    string loopSubET = "";
        //    string loopStoredETString = "";
        //    var tag = "POSITEM";
        //    GetLoopString(tag, template, ref loopSubET, ref loopStoredETString);
        //    var strBuilder = new StringBuilder();
        //    var tmp = "";
        //    foreach (var item in invoiceDTO.InvoiceItemList)
        //    {
        //        tmp = loopSubET;
        //        tmp = tmp.Replace("{ProductName}", item.ProductName ?? "", StringComparison.InvariantCultureIgnoreCase);
        //        tmp = tmp.Replace("{Quantity}", item.Quantity ?? "", StringComparison.InvariantCultureIgnoreCase);
        //        tmp = tmp.Replace("{ProductPrice}", item.SalesPrice ?? "", StringComparison.InvariantCultureIgnoreCase);
        //        tmp = tmp.Replace("{TotalPrice}", item.TotalPrice ?? "", StringComparison.InvariantCultureIgnoreCase);

        //        strBuilder.Append(tmp);
        //    }
        //    template = template.Replace("{Subtotal}", invoiceDTO.Subtotal.ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{Tax}", invoiceDTO.TotalTax.ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{Total}", invoiceDTO.TotalPrice.ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{CompanyName}", invoiceDTO.CompanyName.ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{CompanyAddress}", invoiceDTO.CompanyAddress.ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{CompanyEmail}", invoiceDTO.CompanyEmail.ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{CompanyWebsite}", invoiceDTO.CompanyWebsite.ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{CompanyPhone}", invoiceDTO.CompanyPhone.ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{Date}", DateTime.Now.ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{PaymentType}", invoiceDTO.PaymentType, StringComparison.InvariantCultureIgnoreCase)
        //        .Replace("{Receipt}", invoiceDTO.InvoiceNo, StringComparison.InvariantCultureIgnoreCase);

        //    var cash = "CASH";
        //    var strCashBuilder = new StringBuilder();
        //    template = template.Replace(loopStoredETString, strBuilder.ToString(), StringComparison.InvariantCultureIgnoreCase);
        //    GetLoopString(cash, template, ref loopSubET, ref loopStoredETString);
        //    foreach (var item in invoiceDTO.InvoicePaymentList)
        //    {
        //        tmp = loopSubET;
        //        tmp = tmp.Replace("{PaymentTypeC}", item.PaymentType.ToString(), StringComparison.InvariantCultureIgnoreCase);
        //        tmp = tmp.Replace("{Cash}", item.PaidTotal.ToString(), StringComparison.InvariantCultureIgnoreCase);
        //        //tmp = tmp.Replace("{CashChange}", (invoiceDTO.PaidTotal - invoiceDTO.TotalPrice).ToString());
        //        //template = template.Replace(loopStoredETString, tmp);
        //        strCashBuilder.Append(tmp);
        //    }
        //    template = template.Replace(loopStoredETString, strCashBuilder.ToString(), StringComparison.InvariantCultureIgnoreCase);
        //    if (invoiceDTO.InvoicePaymentList.Sum(s => s.PaidTotal) > 0 && invoiceDTO.InvoicePaymentList.Any(s => s.PaymentType == PaymentType.Cash.ToString()))
        //    {
        //        //{ tmp = loopSubET;
        //        //    tmp = tmp.Replace("{Cash}", invoiceDTO.PaidTotal.ToString());
        //        template = template.Replace("{CashChange}", (invoiceDTO.InvoicePaymentList.Sum(s => s.PaidTotal) - invoiceDTO.TotalPrice).ToString(), StringComparison.InvariantCultureIgnoreCase);
        //        //    template = template.Replace(loopStoredETString, tmp);
        //    }
        //    else
        //    {
        //        template = template.Replace("{CashChange}", "", StringComparison.InvariantCultureIgnoreCase);
        //    }

        //    return template;/// template.Replace(loopStoredETString, strBuilder.ToString());
        //}

        //private void GetLoopString(string tag, string loop, ref string loopSubET, ref string loopStoredETString)
        //{
        //    if (loop.IndexOf("#" + tag + "[", StringComparison.InvariantCultureIgnoreCase) > -1)
        //    {
        //        var strlen = tag.Length + 2;
        //        int indexStart = loop.IndexOf("#" + tag + "[", StringComparison.InvariantCultureIgnoreCase) + strlen;
        //        int indexEnd = loop.IndexOf("]" + tag + "#", StringComparison.InvariantCultureIgnoreCase);
        //        int lenght = indexEnd - indexStart;
        //        loopSubET = loop.Substring(indexStart, lenght);
        //        loopStoredETString = loop.Substring(indexStart - strlen, lenght + strlen * 2);
        //    }
        //}

        //private static void AddImageToBody(WordprocessingDocument wordDoc, string relationshipId)
        //{
        //    // Define the reference of the image.
        //    var element =
        //         new Drawing(
        //             new DW.Inline(
        //                 new DW.Extent() { Cx = 990000L, Cy = 792000L },
        //                 new DW.EffectExtent()
        //                 {
        //                     LeftEdge = 0L,
        //                     TopEdge = 0L,
        //                     RightEdge = 0L,
        //                     BottomEdge = 0L
        //                 },
        //                 new DW.DocProperties()
        //                 {
        //                     Id = (UInt32Value)1U,
        //                     Name = "Picture 1"
        //                 },
        //                 new DW.NonVisualGraphicFrameDrawingProperties(
        //                     new A.GraphicFrameLocks() { NoChangeAspect = true }),
        //                 new A.Graphic(
        //                     new A.GraphicData(
        //                         new PIC.Picture(
        //                             new PIC.NonVisualPictureProperties(
        //                                 new PIC.NonVisualDrawingProperties()
        //                                 {
        //                                     Id = (UInt32Value)0U,
        //                                     Name = "New Bitmap Image.jpg"
        //                                 },
        //                                 new PIC.NonVisualPictureDrawingProperties()),
        //                             new PIC.BlipFill(
        //                                 new A.Blip(
        //                                     new A.BlipExtensionList(
        //                                         new A.BlipExtension()
        //                                         {
        //                                             Uri =
        //                                                "{28A0092B-C50C-407E-A947-70E740481C1C}"
        //                                         })
        //                                 )
        //                                 {
        //                                     Embed = relationshipId,
        //                                     CompressionState =
        //                                     A.BlipCompressionValues.Print
        //                                 },
        //                                 new A.Stretch(
        //                                     new A.FillRectangle())),
        //                             new PIC.ShapeProperties(
        //                                 new A.Transform2D(
        //                                     new A.Offset() { X = 0L, Y = 0L },
        //                                     new A.Extents() { Cx = 990000L, Cy = 792000L }),
        //                                 new A.PresetGeometry(
        //                                     new A.AdjustValueList()
        //                                 )
        //                                 { Preset = A.ShapeTypeValues.Rectangle }))
        //                     )
        //                     { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
        //             )
        //             {
        //                 DistanceFromTop = (UInt32Value)0U,
        //                 DistanceFromBottom = (UInt32Value)0U,
        //                 DistanceFromLeft = (UInt32Value)0U,
        //                 DistanceFromRight = (UInt32Value)0U,
        //                 EditId = "50D07946"
        //             });

        //    // Append the reference to body, the element should be in a Run.
        //    wordDoc.MainDocumentPart.Document.Body.AppendChild(new Paragraph(new Run(element)));
        //}

        #endregion -- [ Docs ] --

        #region -- [ Pdf ] --

        //public MemoryStream ExportPDF(InvoiceDTO invoiceDTO)
        //{
        //    var columnsHeader = new List<string>{
        //            "Item",
        //            "#",
        //            //"Customer Id",
        //            "Price",
        //            "Total",
        //    };
        //    var heading = "";
        //    var document = new iTextSharp.text.Document(new iTextSharp.text.Rectangle(300, 612), 0, 0, 0, 0);
        //    var outputMS = new MemoryStream();
        //    var writer = PdfWriter.GetInstance(document, outputMS);
        //    document.Open();
        //    ProcesPdf(invoiceDTO, columnsHeader, heading, document, writer);
        //    document.Add(new iTextSharp.text.Phrase(Environment.NewLine));
        //    document.Add(new iTextSharp.text.Phrase(Environment.NewLine));
        //    ProcesPdf(invoiceDTO, columnsHeader, heading, document, writer);
        //    //var result = outputMS.ToArray();
        //    document.Close();
        //    return outputMS;
        //}

        //private static void ProcesPdf(InvoiceDTO invoiceDTO, List<string> columnsHeader, string heading, iTextSharp.text.Document document, PdfWriter writer)
        //{
        //    var font5 = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 11);
        //    var font4 = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 15);
        //    document.Add(new iTextSharp.text.Phrase(Environment.NewLine));

        //    document.Add(new iTextSharp.text.Paragraph(invoiceDTO.CompanyName.ToString(), font4) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
        //    document.Add(new iTextSharp.text.Paragraph(invoiceDTO.CompanyAddress.ToString(), font5) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
        //    document.Add(new iTextSharp.text.Paragraph(invoiceDTO.CompanyEmail.ToString() + "," + invoiceDTO.CompanyWebsite.ToString(), font5) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
        //    document.Add(new iTextSharp.text.Paragraph(invoiceDTO.CompanyPhone.ToString(), font5) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
        //    document.Add(new iTextSharp.text.Paragraph(DateTime.Now.ToString(), font5) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
        //    document.Add(new iTextSharp.text.Paragraph("Payment Type:" + invoiceDTO.PaymentType.ToString(), font5) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
        //    document.Add(new iTextSharp.text.Phrase(""));
        //    //  document.Add(new iTextSharp.text.Paragraph(invoiceDTO.InvoiceNo, font5) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
        //    var barcode128 = new Barcode128();
        //    barcode128.Code = invoiceDTO.InvoiceNo;
        //    // comment next line to show barcode text
        //    //barcode128.Font = null;
        //    PdfContentByte cb = writer.DirectContent;
        //    iTextSharp.text.Image image1 = barcode128.CreateImageWithBarcode(cb, null, null);
        //    image1.ScaleAbsolute(100, 40);
        //    image1.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
        //    document.Add(image1);

        //    document.Add(new iTextSharp.text.Phrase(""));

        //    #region Main table

        //    var count = columnsHeader.Count;
        //    var table = new PdfPTable(count);
        //    float[] widths = new float[] { 7f, 1f, 1.8f, 1.8f };

        //    table.SetWidths(widths);

        //    table.WidthPercentage = 100;
        //    var cell = new PdfPCell(new iTextSharp.text.Phrase(heading));
        //    cell.Colspan = count;

        //    for (int i = 0; i < count; i++)
        //    {
        //        var headerCell = new PdfPCell(new iTextSharp.text.Phrase(columnsHeader[i], font5));
        //        headerCell.Border = 0;// iTextSharp.text.BaseColor.Gray;
        //        headerCell.HorizontalAlignment = i == 0 ? 0 : 2;
        //        table.AddCell(headerCell);
        //    }
        //    foreach (var item in invoiceDTO.InvoiceItemList)
        //    {
        //        table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(item.ProductName, font5)) { Border = 0, HorizontalAlignment = 0 });
        //        table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(item.Quantity, font5)) { Border = 0, HorizontalAlignment = 2 });
        //        table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(item.SalesPrice.ToString(), font5)) { Border = 0, HorizontalAlignment = 2 });
        //        table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(item.TotalPrice, font5)) { Border = 0, HorizontalAlignment = 2 });
        //    }

        //    document.Add(table);

        //    #endregion Main table

        //    document.Add(new iTextSharp.text.Phrase(""));

        //    #region footer table

        //    var ftable = new PdfPTable(2);
        //    float[] fwidths = new float[] { 9f, 2f };

        //    ftable.SetWidths(fwidths);
        //    //ftable.TotalWidth = 100f;
        //    //ftable.LockedWidth = true;
        //    ftable.WidthPercentage = 100;

        //    ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase("Subtotal", font5)) { Border = 0, HorizontalAlignment = 2 });
        //    ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase(invoiceDTO.Subtotal.ToString(), font5)) { Border = 0, HorizontalAlignment = 2 });
        //    ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase("Tax", font5)) { Border = 0, HorizontalAlignment = 2 });
        //    ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase(invoiceDTO.TotalTax.ToString(), font5)) { Border = 0, HorizontalAlignment = 2 });
        //    ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase("Total", font5)) { Border = 0, HorizontalAlignment = 2 });
        //    ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase(invoiceDTO.TotalPrice.ToString(), font5)) { Border = 0, HorizontalAlignment = 2 });
        //    foreach (var item in invoiceDTO.InvoicePaymentList)
        //    {
        //        ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase(item.PaymentType.ToString(), font5)) { Border = 0, HorizontalAlignment = 2 });
        //        ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase(item.PaidTotal.ToString(), font5)) { Border = 0, HorizontalAlignment = 2 });
        //    }
        //    if (invoiceDTO.InvoicePaymentList.Sum(s => s.PaidTotal) > 0 && invoiceDTO.InvoicePaymentList.Any(s => s.PaymentType == PaymentType.Cash.ToString()))
        //    {
        //        ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase("Cash Change", font5)) { Border = 0, HorizontalAlignment = 2 });
        //        ftable.AddCell(new PdfPCell(new iTextSharp.text.Phrase((invoiceDTO.InvoicePaymentList.Sum(s => s.PaidTotal) - invoiceDTO.TotalPrice).ToString(), font5)) { Border = 0, HorizontalAlignment = 2 });
        //    }

        //    document.Add(ftable);

        //    #endregion footer table
        //}

        #endregion -- [ Pdf ] --

        #region -- [ Raw printing ] --

        //Ref
        //https://reference.epson-biz.com/modules/ref_escpos/index.php?content_id=270
        //https://reference.epson-biz.com/modules/ref_escpos/index.php?content_id=128
        //https://www.neodynamic.com/articles/How-to-print-raw-ESC-POS-commands-from-ASP-NET-directly-to-the-client-printer/#v4
        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/d1dd6b1b-62c8-4ca8-9822-607e277d01f6/pos-bill-printing-using-opos?forum=posfordotnet
        //https://www.sparkfun.com/datasheets/Components/General/Driver%20board.pdf
        private string NewLine = "0x0A";

        //private string NewLine = "\r\n";
        /// <summary>
        /// Print Receipt
        /// </summary>
        /// <param name="invoiceDTO"></param>
        /// <param name="copy"></param>
        /// <returns></returns>
        public string PrintReceipt(InvoiceDTO invoiceDTO, string receiptType, int copy = 1)
        {
            try
            {
                string ESC = "0x1B"; //ESC byte in hex notation
                                     // string NewLine = "0x0A"; //LF byte in hex notation
                string GS = "0x1D";
                string cmds = ESC + "@"; //Initializes the printer (ESC @)
                                         //cmds += ESC + "!" + "0x38";
                                         //cmds += ESC + "!" + "0x00";

                #region print header, details, footer

                invoiceDTO = invoiceDTO ?? throw new ArgumentNullException(nameof(invoiceDTO));
                cmds += PrintReceiptHeader(invoiceDTO.CompanyName, invoiceDTO.CompanyAddress, invoiceDTO.CompanyAddress2, invoiceDTO.CompanyPhone, invoiceDTO.CreatedDate, invoiceDTO.CreatedBy, receiptType);
                foreach (var item in invoiceDTO.InvoiceItemList)
                {
                    int quantity = (int)(double.Parse(item.Quantity));
                    if (string.IsNullOrEmpty(item.OverriddenPrice))
                    {
                        cmds += PrintLineItem(item.ProductName, item.RefPrescriptionId, quantity, double.Parse(item.SalesPrice),
                            double.Parse(item.ItemTotalDiscount), double.Parse(item.TotalPrice));
                    }
                    else
                    {
                        double totalPrice = item.ProductId.ToString() == "0" ? double.Parse(item.OverriddenPrice) : double.Parse(item.OverriddenPrice) * int.Parse(item.Quantity);
                        cmds += PrintLineItem(item.ProductName, item.RefPrescriptionId, quantity, double.Parse(item.OverriddenPrice), double.Parse(item.ItemTotalDiscount), totalPrice);
                    }
                }
                cmds += PrintReceiptFooter(invoiceDTO.Subtotal, invoiceDTO.TotalTax, invoiceDTO.Discount.Value, invoiceDTO.TotalPrice, invoiceDTO);

                #endregion print header, details, footer

                //cmds += PrintLineItem("ASDFSD FSDFSDF SDF 4", 1000, 1);
                cmds += ESC + "a" + "0x01";
                cmds += ESC + "J" + "0x23";

                cmds += GS + "h" + "0x32";

                cmds += GS + "H" + "0x02";

                cmds += GS + "f" + "0x01";
                cmds += GS + "k" + "0x04" + invoiceDTO.InvoiceNo + "0x00";
                cmds += GS + "H" + "0x00";
                cmds += NewLine + NewLine + NewLine;
                //cmds += PrintGraphics() + NewLine + NewLine + NewLine;
                cmds += "0x0A0x1D0x560x000x0A";

                var temp = cmds;

                for (int i = 1; i < copy; i++)
                {
                    cmds += temp;
                }

                cmds += "0x10" + "0x14" + "0x01" + "0x00" + "0x05"; //Open Cash Drawer Command

                return cmds;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Print Receipt Footer
        /// </summary>
        /// <param name="subTotal"></param>
        /// <param name="tax"></param>
        /// <param name="discount"></param>
        /// <param name="total"></param>
        /// <param name="invoiceDTO"></param>
        /// <returns></returns>
        private string PrintReceiptFooter(double subTotal, double tax, double discount, double total, InvoiceDTO invoiceDTO)
        {
            var RecLineChars = 48;
            var offSetString = new string(' ', RecLineChars / 2);
            var cmds = "";
            cmds += PrintTextLine(new string('-', RecLineChars));
            // cmds += PrintTextLine(offSetString + String.Format("SUB-TOTAL     {0}", subTotal.ToString("#0.00")));
            cmds += PrintText(offSetString + TruncateAt("SUB-TOTAL".PadRight(12), 12));
            cmds += PrintText(TruncateAt(subTotal.ToString("N2").PadLeft(12), 12));
            cmds += NewLine;
            cmds += PrintText(offSetString + TruncateAt("TAX".PadRight(12), 12));
            cmds += PrintText(TruncateAt(tax.ToString("N2").PadLeft(12), 12));
            cmds += NewLine;
            cmds += PrintText(offSetString + TruncateAt("DISCOUNT".PadRight(12), 12));
            cmds += PrintText(TruncateAt(discount.ToString("N2").PadLeft(12), 12));
            cmds += NewLine;
            cmds += PrintTextLine(offSetString + new string('-', (RecLineChars / 2)));
            cmds += PrintText(offSetString + TruncateAt("TOTAL".PadRight(12), 12));
            cmds += PrintText(TruncateAt(total.ToString("N2").PadLeft(12), 12));
            cmds += NewLine;
            cmds += PrintTextLine(offSetString + new string('-', (RecLineChars / 2)));

            foreach (var item in invoiceDTO.InvoicePaymentList)
            {
                if (item.PaymentType == "Cash")
                {
                    cmds += PrintText(offSetString + TruncateAt("Cash Tendered".PadRight(13), 13));
                }
                else
                {
                    cmds += PrintText(offSetString + TruncateAt(item.PaymentType.PadRight(12), 12));
                }

                if (item.PaymentType == "Cash")
                {
                    cmds += PrintText(TruncateAt(item.PaidTotal.ToString("N2").PadLeft(11), 11));
                }
                else
                {
                    cmds += PrintText(TruncateAt(item.PaidTotal.ToString("N2").PadLeft(12), 12));
                }
                cmds += NewLine;
            }

            if (invoiceDTO.InvoicePaymentList.Sum(s => s.PaidTotal) > 0 && invoiceDTO.InvoicePaymentList.Any(s => s.PaymentType == "Cash"))
            {
                cmds += PrintText(offSetString + TruncateAt("Cash Due".PadRight(12), 12));
                cmds += PrintText(TruncateAt((invoiceDTO.InvoicePaymentList.Sum(s => s.PaidTotal) - invoiceDTO.TotalPrice).ToString("N2").PadLeft(12), 12));
                cmds += NewLine;
            }

            if (invoiceDTO.InvoicePaymentList.Sum(s => s.PaidTotal) > 0 && invoiceDTO.InvoicePaymentList.Any(s => s.PaymentType == "Card"))
            {
                cmds += PrintText(offSetString + TruncateAt("Card #".PadRight(8), 8));
                cmds += PrintText(invoiceDTO.Masked_Account);
                cmds += NewLine;
            }

            if (invoiceDTO.LoyaltyPointEarned != null)
            {
                if (int.Parse(invoiceDTO.LoyaltyPointEarned) > 0)
                {
                    cmds += PrintText(offSetString + TruncateAt("Loyalty Points".PadRight(14), 14));
                    cmds += PrintText(invoiceDTO.LoyaltyPointEarned.PadLeft(10));
                    cmds += NewLine;
                }
            }

            cmds += Notes;

            return cmds;
        }

        /// <summary>
        /// Print Line Item
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="quantity"></param>
        /// <param name="unitPrice"></param>
        /// <returns></returns>
        private string PrintLineItem(string itemCode, string rx, int quantity, double unitPrice, double discount, double totalPrice)
        {
            var cmds = "";

            if (!string.IsNullOrEmpty(rx))
            {
                if (!_printOnlyRx)
                    cmds += PrintTextLine(TruncateAt(itemCode.PadRight(24), 24));

                cmds += PrintText(TruncateAt(("Rx#:" + rx).PadRight(15), 15));
                cmds += PrintText(TruncateAt(quantity.ToString().PadLeft(3), 3));
                cmds += PrintText(TruncateAt(unitPrice.ToString("N2").PadLeft(12), 12));
                cmds += PrintText(TruncateAt((discount).ToString("N2").PadLeft(8), 8));
                cmds += PrintTextLine(TruncateAt((totalPrice).ToString("N2").PadLeft(10), 10));
            }
            else
            {
                cmds += PrintTextLine(TruncateAt(itemCode.PadRight(24), 24));

                cmds += PrintText(TruncateAt("".PadRight(14), 14));
                cmds += PrintText(TruncateAt(quantity.ToString().PadLeft(4), 4));
                cmds += PrintText(TruncateAt(unitPrice.ToString("N2").PadLeft(10), 10));
                cmds += PrintText(TruncateAt((discount).ToString("N2").PadLeft(10), 10));
                cmds += PrintTextLine(TruncateAt((totalPrice).ToString("N2").PadLeft(10), 10));
            }

            return cmds;
        }

        /// <summary>
        /// Print Receipt Header
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="addressLine1"></param>
        /// <param name="addressLine2"></param>
        /// <param name="taxNumber"></param>
        /// <param name="dateTime"></param>
        /// <param name="cashierName"></param>
        /// <returns></returns>
        private string PrintReceiptHeader(string companyName, string addressLine1, string addressLine2, string taxNumber, DateTime dateTime, string cashierName, string receiptType)
        {
            var RecLineChars = 48;
            string ESC = "0x1B";
            var cmds = "";
            cmds += ESC + "!" + "0x18";// "0x0C"

            cmds += PrintTextLine(PadBoth(companyName, RecLineChars));
            cmds += ESC + "!" + "0x00";
            cmds += PrintTextLine(PadBoth(addressLine1, RecLineChars));
            cmds += PrintTextLine(PadBoth(addressLine2, RecLineChars));
            cmds += PrintTextLine(PadBoth(taxNumber, RecLineChars));

            cmds += PrintTextLine(new string('-', RecLineChars));

            cmds += PrintTextLine(String.Format("DATE : {0}", dateTime));
            cmds += PrintTextLine(String.Format("CASHIER : {0}", cashierName));
            cmds += PrintTextLine(String.Format("Transaction Type: {0}", receiptType));
            cmds += PrintTextLine(String.Empty);
            cmds += PrintTextLine("Item                      ");
            cmds += PrintText("               ");
            cmds += PrintText("Qty  ");
            cmds += PrintText("Unit Price ");
            cmds += PrintText("Discount");
            cmds += PrintTextLine("    Total");
            cmds += PrintTextLine(new string('=', RecLineChars));
            // cmds += PrintTextLine(String.Empty);
            return cmds;
        }

        /// <summary>
        /// Print Text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string PrintText(string text)
        {
            //if (text.Length <= printer.RecLineChars)
            //    printer.PrintNormal(PrinterStation.Receipt, text); //Print text
            //else if (text.Length > printer.RecLineChars)
            //    printer.PrintNormal(PrinterStation.Receipt, TruncateAt(text, printer.RecLineChars)); //Print exactly as many characters as the printer allows, truncating the rest.
            return text;
        }

        /// <summary>
        /// Print Text Line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string PrintTextLine(string text)
        {
            return text + NewLine;
        }

        /// <summary>
        /// Truncate At
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        private string TruncateAt(string text, int maxWidth)
        {
            string retVal = text;
            if (text.Length > maxWidth)
                retVal = text.Substring(0, maxWidth);

            return retVal;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private string PadBoth(string text, int length)
        {
            int spaces = length - text.Length;
            int padLeft = spaces / 2 + text.Length;
            return text.PadLeft(padLeft).PadRight(length);
        }

        /// <summary>
        /// Create Invoice
        /// </summary>
        /// <returns></returns>
        private InvoiceDTO CreateInvoice()
        {
            var invoiceDTO = new InvoiceDTO();
            invoiceDTO.CompanyName = "DAA Enterprises Inc.";
            invoiceDTO.CompanyAddress = "369 Harvard Street,Suite -1";
            invoiceDTO.CompanyAddress2 = "Brookline, MA 02446";
            invoiceDTO.CompanyEmail = "(800) 359-5580";
            invoiceDTO.CompanyPhone = "info@daaenterprises.com";
            invoiceDTO.CompanyWebsite = "https://daaenterprises.com/";
            invoiceDTO.InvoiceNo = "0000048";
            invoiceDTO.InvoiceItemList = new List<InvoiceItemDTO>();
            invoiceDTO.InvoiceItemList.Add(new InvoiceItemDTO { ProductName = "Invo iceIt emDTO", Quantity = "6", SalesPrice = "10", TotalPrice = "60" });
            invoiceDTO.InvoiceItemList.Add(new InvoiceItemDTO { ProductName = "KHGHFGF DFDG DFD G FG", Quantity = "6", SalesPrice = "10", TotalPrice = "60" });
            invoiceDTO.InvoiceItemList.Add(new InvoiceItemDTO { ProductName = "YYUYU  FXDDXGF HGHG JHGH", Quantity = "6", SalesPrice = "10", TotalPrice = "60" });
            invoiceDTO.InvoiceItemList.Add(new InvoiceItemDTO { ProductName = "IUYY YUTUTYT TYTYT", Quantity = "6", SalesPrice = "10", TotalPrice = "60" });

            invoiceDTO.InvoicePaymentList = new List<InvoicePaymentDTO>();
            invoiceDTO.InvoicePaymentList.Add(new InvoicePaymentDTO { PaidTotal = 1210, PaymentType = "Cash" });
            invoiceDTO.InvoicePaymentList.Add(new InvoicePaymentDTO { PaidTotal = 210, PaymentType = "Card" });
            //invoiceDTO.InvoicePaymentList.Add(new InvoicePaymentDTO { PaidTotal = 110, PaymentType = "Cash" });
            return invoiceDTO;
        }

        public string PrintGraphics()
        {
            string ESC = Convert.ToString((char)27);
            string LF = Convert.ToString((char)10);
            string GS = Convert.ToString((char)29);
            string cutCommand = $"{ESC}@{GS}V{(char)66}{(char)0}";
            byte[] commandBytes = Encoding.ASCII.GetBytes($"{ESC}@{GS}{(char)40}{(char)76}{(char)139}{(char)7}{(char)48}{(char)67}{(char)48}G1{(char)1}{(char)128}{(char)0}{(char)120}{(char)0}{(char)49}");
            //byte[] commandBytes = Encoding.Unicode.GetBytes($"{ESC} (L 139 7 48 67 48 G1 1 128 0 120 0 49");
            byte[] imageBytes = System.IO.File.ReadAllBytes("E:/a.bmp"); ;// Example.GetExampleImageBytes();
            byte[] commandEndBytes = Encoding.ASCII.GetBytes($"{ESC}@{GS}{(char)40}{(char)76}{(char)6}{(char)0}{(char)48}{(char)69}G1{(char)1}{(char)1}");
            byte[] endBytes = commandBytes.Concat(imageBytes).Concat(commandEndBytes).ToArray();
            //RawPrinterHelper.SendBytesToPrinter(printerName, endBytes);
            //RawPrinterHelper.SendBytesToPrinter(printerName, Encoding.ASCII.GetBytes(cutCommand));
            var hex = new StringBuilder(endBytes.Length * 2);
            foreach (byte b in endBytes)
                hex.AppendFormat(" 0x{0:x2}", b);
            return hex.ToString();
        }

        #endregion -- [ Raw printing ] --
    }
}