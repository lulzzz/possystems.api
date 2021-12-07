CREATE TABLE [dbo].[company](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NOT NULL,
	[address] [nvarchar](100) NOT NULL,
	[address2] [nvarchar](100) NULL,
	[phone] [nvarchar](50) NOT NULL,
	[email] [nvarchar](50) NOT NULL,
	[website] [nvarchar](50) NULL,
	[template] [nvarchar](max) NOT NULL,
	[smtp_server] [nvarchar](50) NOT NULL,
	[smtp_user] [nvarchar](50) NOT NULL,
	[smtp_password] [nvarchar](100) NOT NULL,
	[created_date] [datetime] NOT NULL,
	[created_by] [nvarchar](50) NOT NULL,
	[modified_date] [datetime] NULL,
	[modified_by] [nvarchar](50) NULL,
	[status] [char](1) NOT NULL,
	[smtp_port] [int] NULL,
 CONSTRAINT [PK_company] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


SET IDENTITY_INSERT [dbo].[company] ON 
GO

INSERT [dbo].[company] ([id], [name], [address], [address2], [phone], [email], [website], [template], [smtp_server], [smtp_user], [smtp_password], [created_date], [created_by], [modified_date], [modified_by], [status]) VALUES (1, N'DAA', N'369 Harvard Street', N'Brookline, Massachusetts 02446', N'(800) 359-5580', N'info@daaenterprises.com', N'https://daaenterprises.com/', N'<table class="table table-hover" style="width: 295px; text-align: center;">         <tr>             <td>                 <h2>{CompanyName}</h2>             </td>         </tr>         <tr>             <td>{CompanyAddress}                                                       <abbr title="Phone">P:</abbr>                 {CompanyPhone}                                               <br />                 Date: {Date}                                 <br />   Payment Type #:{PaymentType}    <br />          Receipt #: {Receipt} <br/> <img src="{ImgUrl}"   ></td>         </tr>     </table>     <table class="table table-hover" style="width: 275px;">         <thead>             <tr>                 <th style="border-style: 2; border-width: 2px; text-align: left; padding: 2px; margin: 2px" class="auto-style2">Product</th>                 <th class="auto-style4" style="padding: 2px; margin: 2px; text-align: right">#</th>                 <th class="text-center" style="padding: 2px; margin: 2px; text-align: right">Price</th>                 <th class="auto-style3" style="padding: 2px; margin: 2px; text-align: right">Total</th>             </tr>         </thead>         <tbody>             #POSITEM[<tr>                 <td class="auto-style2" style="border-style: 2; border-width: 2px; text-align: left; padding: 2px; margin: 2px; font-size: 12px"><em>{ProductName}</em></h4></td>                 <td class="auto-style4" style="padding: 2px; margin: 2px; text-align: right; font-size: 12px">{Quantity} </td>                 <td class="col-md-1 text-center" style="padding: 2px; margin: 2px; text-align: right; font-size: 12px">{ProductPrice}</td>                 <td class="auto-style3" style="padding: 2px; margin: 2px; text-align: right; font-size: 12px">{TotalPrice}</td>             </tr>             ]POSITEM#                       <tr>                 <td style="border-style: 2; border-width: 2px; text-align: left; padding: 2px; margin: 2px" class="auto-style2"></td>                 <td class="auto-style4" style="padding: 2px; margin: 2px; text-align: right"></td>                 <td class="text-right" style="padding: 2px; margin: 2px; text-align: right">                     <p>Subtotal:</p>                     <p>Tax:</p>                 </td>                 <td class="auto-style3" style="padding: 2px; margin: 2px; text-align: right">                     <p>{Subtotal}</p>                     <p>{Tax}</p>                 </td>             </tr>             <tr>                 <td style="border-style: 2; border-width: 2px; text-align: left; padding: 2px; margin: 2px" class="auto-style2"></td>                 <td class="auto-style4" style="padding: 2px; margin: 2px; text-align: right"></td>                 <td class="text-right" style="padding: 2px; margin: 2px; text-align: right">                     <p>Total:</p>                 </td>                 <td class="auto-style3" style="padding: 2px; margin: 2px; text-align: right">                     <p>{Total}</p>                 </td>             </tr> #CASH[ <tr> <td style="border-style: 2; border-width: 2px; text-align: left; padding: 2px; margin: 2px" class="auto-style2"></td>  <td class="text-right" style="padding: 2px; margin: 2px; text-align: right" colspan="2" >{PaymentTypeC}:</td> <td class="auto-style3" style="padding: 2px; margin: 2px; text-align: right">{Cash}</td> </tr> ]CASH# <tr> <td style="border-style: 2; border-width: 2px; text-align: left; padding: 2px; margin: 2px" class="auto-style2"></td> <td class="text-right" style="padding: 2px; margin: 2px; text-align: right" colspan="2" >Cash Change:</td> <td class="auto-style3" style="padding: 2px; margin: 2px; text-align: right">{CashChange}</td> </tr>           </tbody>     </table>', N'', N'', N'', CAST(N'2020-11-06T13:25:00.167' AS DateTime), N'admin', CAST(N'2020-11-06T13:25:00.167' AS DateTime), N'admin', 'A')
GO
SET IDENTITY_INSERT [dbo].[company] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_company]    Script Date: 11/6/2020 1:28:50 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_company] ON [dbo].[company]
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
