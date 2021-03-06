ALTER FUNCTION [dbo].[select_price]
(
	@purchasePrice varchar(50),
	@salesPrice varchar(50),
	@productPriceRangeId int
)
RETURNS decimal(10,2)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @selectedPrice numeric(10,2), @parsedPurchasePrice decimal(10,2),@parsedSalesPrice decimal(10,2) = 0, @costPreference char(1), @markup decimal(10,2);

	set @parsedPurchasePrice = TRY_PARSE(@purchasePrice AS decimal(10,2));
	if (@parsedPurchasePrice IS not NULL and @productPriceRangeId Is null)
	begin
		RETURN @parsedPurchasePrice;
	end
	else if (@parsedPurchasePrice IS NULL and @productPriceRangeId Is null)
	begin
		return null;	
	end

	set @parsedSalesPrice = TRY_PARSE(@salesPrice AS decimal(10,2));
	if (@parsedSalesPrice IS NULL)
	begin
		return null;	
	end

	if (@productPriceRangeId Is null)
	begin
		return @parsedSalesPrice;
	end

	set @costPreference = (select cost_preference from product_pricerange where product_pricerange_id = @productPriceRangeId)
	if @costPreference = 'Q'
	begin 
	set @selectedPrice = @parsedPurchasePrice
	end
	else if @costPreference = 'W'
	begin
	set @selectedPrice = @parsedSalesPrice
	end

	set @markup = (select markup from pricerange pr join product pm on pr.product_pricerange_id = pm.product_pricerange_id and @selectedPrice between minrange and maxrange);

	set @selectedPrice = @selectedPrice * @markup;
	RETURN (@selectedPrice);

END