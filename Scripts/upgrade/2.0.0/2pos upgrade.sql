ALTER TABLE [dbo].[product_detail]  DROP CONSTRAINT FK_product_detail_Supplier;    
ALTER TABLE [dbo].[product_detail]  DROP CONSTRAINT FK_product_details_measurement_unit;     
ALTER TABLE [dbo].[product_detail]  DROP CONSTRAINT FK_product_details_product_info;     
ALTER TABLE [dbo].[product_detail]  DROP CONSTRAINT FK_product_details_Supplier_info;     

ALTER TABLE [dbo].[product_detail] ADD category_id int null;
ALTER TABLE [dbo].[product_detail] ADD product_name nvarchar(150) null;
ALTER TABLE [dbo].[product_detail] ADD min_stock int null;
ALTER TABLE [dbo].[product_detail] ADD max_stock int null;
ALTER TABLE [dbo].[product_detail] ADD reorder_level int null;
ALTER TABLE [dbo].[product_detail] ADD catalog_product_id nvarchar(20) null;
ALTER TABLE [dbo].[product_detail] ADD product_pricerange_id int null;

ALTER TABLE [dbo].[product_detail]  ADD CONSTRAINT FK_product_supplier FOREIGN KEY(supplier_id) REFERENCES [dbo].[supplier](supplier_id);
ALTER TABLE [dbo].[product_detail]  ADD CONSTRAINT FK_product_measurement_unit FOREIGN KEY(measurement_id) REFERENCES [dbo].[measurement_unit](measurement_id);
ALTER TABLE [dbo].[product_detail]  ADD CONSTRAINT FK_product_category FOREIGN KEY(category_id) REFERENCES [dbo].[product_category](category_id);

GO

UPDATE p
SET p.category_id = pmc.category_id,
p.product_name = pmc.product_name,
p.min_stock = pmc.min_stock,
p.max_stock = pmc.max_stock,
p.reorder_level = pmc.reorder_level,
p.catalog_product_id = pmc.catalog_product_id,
P.product_pricerange_id = pmc.product_pricerange_id
from dbo.product_detail AS p 
   INNER JOIN 
   (select pm.category_id, 
   pd.product_detail_id, 
   pm.product_name, 
   pm.product_pricerange_id,
   pm.catalog_product_id,
   pm.reorder_level,
   pm.max_stock,
   pm.min_stock
   from dbo.product_detail pd 
   inner join dbo.product_master pm 
   on pd.product_id = pm.product_id) AS pmc
   ON p.product_detail_id = pmc.product_detail_id;

ALTER TABLE product_detail DROP COLUMN product_id; 
exec  sp_rename 'product_detail', 'product';
exec  sp_rename 'product.product_detail_id', 'product_id', 'COLUMN';
exec  sp_rename 'sales_detail.product_detail_id', 'product_id', 'COLUMN';
exec  sp_rename 'sales_return.product_detail_id', 'product_id', 'COLUMN';

ALTER TABLE [dbo].[purchase_master] ADD purchase_method nvarchar(50) null;
exec  sp_rename 'purchase_detail.product_detail_id', 'product_id', 'COLUMN';

drop table product_master;


