USE [POSSystemsDBV2]
GO
/****** Object:  StoredProcedure [dbo].[ImportProducts]    Script Date: 10/20/2020 7:50:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Batch submitted through debugger: SQLQuery2.sql|7|0|C:\Users\Illusionist\AppData\Local\Temp\~vsF95F.sql
-- Batch submitted through debugger: SQLQuery60.sql|7|0|C:\Users\Illusionist\Documents\SQL Server Management Studio\SQLQuery60.sql
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[ImportProducts]
	@fileId int,
	@supplierId int,
	@directSalesPrice bit,
	@user_name nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @upcCode Varchar(20)
      ,@scanCode Varchar(20)
      ,@category Varchar(100)
      ,@categoryDescription Varchar(100)
      ,@productName Varchar(100)
      ,@productDescription Varchar(100)
      ,@form Varchar(50)
      ,@strength Varchar(50)
      ,@unit Varchar(50)
      ,@packageSize Varchar(50)
      ,@purchasePrice Varchar(20)
      ,@salesPrice Varchar(20)
      ,@manufacturer Varchar(100)
      ,@itemno Varchar(50)
	  ,@quantity int;
 
	DECLARE ProductCatalogCursor CURSOR FOR
	SELECT [upc_code]
      ,[scan_code]
      ,[category]
      ,[category_description]
      ,[product_name]
      ,[product_description]
      ,[form]
      ,[strength]
      ,[unit]
      ,[package_size]
      ,[purchase_price]
      ,[sales_price]
      ,[manufacturer]
      ,[itemno]
	  ,[quantity] FROM price_catalog
	  where file_id = @fileId
 
	OPEN ProductCatalogCursor
 
	FETCH NEXT FROM ProductCatalogCursor INTO   
      @upcCode
      ,@scanCode
      ,@category
      ,@categoryDescription
      ,@productName
      ,@productDescription
      ,@form
      ,@strength
      ,@unit
      ,@packageSize
      ,@purchasePrice
      ,@salesPrice
      ,@manufacturer
      ,@itemno
	  ,@quantity
		DECLARE @selectedSalesPrice decimal(10,2), @pRealExists int, @prodId int = 0, @priceRangeId int = 0
	WHILE(@@FETCH_STATUS = 0)
	BEGIN
		BEGIN TRY		

			--THROW 51000, 'The record does not exist.', 1;  
			set @pRealExists = (select 1 from product where upc_code = @upcCode);

			if(@pRealExists is null AND @scanCode is not null)
			begin
				select @priceRangeId = product_pricerange_id from product where product_id = @prodId;

				if(@directSalesPrice = 0)
					select @selectedSalesPrice = dbo.select_price(@purchasePrice, @salesPrice, @priceRangeId); 
				else
					select @selectedSalesPrice = @salesPrice;
			
				if 	@selectedSalesPrice is not null	  	
				begin	
					INSERT INTO [dbo].[product]
								([product_name]
								,[category_id]
								,[description]
								,[measurement_id]
								,[supplier_id]
								,[created_date]
								,[created_by]
								,[modified_date]
								,[modified_by]
								,[status]
								,[upc_code]
								,[upc_scan_code]
								,[package_size]
								,[strength]
								,[itemno]
								,purchase_price
								,sales_price
								,quantity
								,tax_ind
								,manufacturer_id)
							VALUES
								(@productName
								,(select category_id from product_category where category_name = @category)
								,@productDescription
								,(select measurement_id from measurement_unit where measurement_name = @unit)			   
								,@supplierId
								,SYSDATETIME()
								,@user_name
								,SYSDATETIME()
								,@user_name						
								,'A'
								,@upcCode
								,@scanCode
								,@packageSize
								,@strength
								,@itemno
								,@purchasePrice
								,@selectedSalesPrice
								,@quantity
								,0
								,(select manufacturer_id from manufacturer where [name] = @manufacturer));
					   
					UPDATE price_catalog
						SET    imported = 1
						WHERE  CURRENT OF ProductCatalogCursor;	
				end
				else
				begin
					UPDATE price_catalog
						SET    error = 'Cannot be imported because price is not parseable'
						WHERE  CURRENT OF ProductCatalogCursor;
				end
				  				
			end
			else
			begin
				UPDATE [dbo].[product]
					SET [product_name] = @productName
					,[category_id] = (select category_id from product_category where category_name = @category)
					,[measurement_id] = (select measurement_id from measurement_unit where measurement_name = @unit)
					,[upc_scan_code] = @scanCode
					,manufacturer_id = (select manufacturer_id from manufacturer where [name] = @manufacturer)
					where upc_code = @upcCode

				UPDATE price_catalog
					  SET    imported = 1
					  WHERE  CURRENT OF ProductCatalogCursor;
			end
		END TRY
		BEGIN CATCH
			UPDATE price_catalog
			  SET    Error = ERROR_NUMBER()
			  WHERE  CURRENT OF ProductCatalogCursor;
		END CATCH
		FETCH NEXT FROM ProductCatalogCursor INTO 
		   @upcCode
		  ,@scanCode
		  ,@category
		  ,@categoryDescription
		  ,@productName
		  ,@productDescription
		  ,@form
		  ,@strength
		  ,@unit
		  ,@packageSize
		  ,@purchasePrice
		  ,@salesPrice
		  ,@manufacturer
		  ,@itemno
		  ,@quantity
	end
	CLOSE ProductCatalogCursor
	DEALLOCATE ProductCatalogCursor
	
END