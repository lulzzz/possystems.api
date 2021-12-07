ALTER TABLE [dbo].[product]
ADD manufacturer_id int null;

ALTER TABLE [dbo].[product]  WITH CHECK ADD  CONSTRAINT [FK_product_manufacturer] FOREIGN KEY([manufacturer_id])
REFERENCES [dbo].[manufacturer] ([manufacturer_id])
GO

ALTER TABLE [dbo].[product] CHECK CONSTRAINT [FK_product_manufacturer]
GO

ALTER TABLE [dbo].[product]  WITH CHECK ADD  CONSTRAINT [FK_product_product_category] FOREIGN KEY([category_id])
REFERENCES [dbo].[product_category] ([category_id])
GO

ALTER TABLE [dbo].[product] CHECK CONSTRAINT [FK_product_product_category]
GO