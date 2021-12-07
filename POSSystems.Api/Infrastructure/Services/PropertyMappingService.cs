using POSSystems.Core.Dtos;
using POSSystems.Core.Dtos.Configuration;
using POSSystems.Core.Dtos.Customer;
using POSSystems.Core.Dtos.EligibleProduct;
using POSSystems.Core.Dtos.Manufacturer;
using POSSystems.Core.Dtos.MeasurementUnit;
using POSSystems.Core.Dtos.PosTerminal;
using POSSystems.Core.Dtos.PriceRange;
using POSSystems.Core.Dtos.Product;
using POSSystems.Core.Dtos.ProductCategory;
using POSSystems.Core.Dtos.PurchaseDetail;
using POSSystems.Core.Dtos.PurchaseMaster;
using POSSystems.Core.Dtos.PurchaseReturn;
using POSSystems.Core.Dtos.Report;
using POSSystems.Core.Dtos.Role;
using POSSystems.Core.Dtos.RoleClaim;
using POSSystems.Core.Dtos.SalesDetail;
using POSSystems.Core.Dtos.SalesMaster;
using POSSystems.Core.Dtos.SalesReturn;
using POSSystems.Core.Dtos.Session;
using POSSystems.Core.Dtos.Supplier;
using POSSystems.Core.Dtos.Transaction;
using POSSystems.Core.Dtos.UserRole;
using POSSystems.Core.Models;
using POSSystems.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POSSystems.Web.Infrastructure.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _measurementUnitPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "MeasurementId" }) },
                { "measurementName", new PropertyMappingValue(new List<string>() { "MeasurementName" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _configurationPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "Id" }) },
                { "configCode", new PropertyMappingValue(new List<string>() { "ConfigCode" }) },
                { "configValue", new PropertyMappingValue(new List<string>() { "ConfigValue" }) },
                { "description", new PropertyMappingValue(new List<string>() { "Description" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _eligibleProductPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "UPC" }) },
                { "gtin", new PropertyMappingValue(new List<string>() { "GTIN" }) },
                { "flc", new PropertyMappingValue(new List<string>() { "FLC" }) },
                { "manufacturerName", new PropertyMappingValue(new List<string>() { "ManufacturerName" }) },
                { "categoryDescription", new PropertyMappingValue(new List<string>() { "CategoryDescription" }) },
                { "subCategoryDescription", new PropertyMappingValue(new List<string>() { "SubCategoryDescription" }) },
                { "finestCategoryDescription", new PropertyMappingValue(new List<string>() { "FinestCategoryDescription" }) },
                { "changeDate", new PropertyMappingValue(new List<string>() { "ChangeDate" }) },
                { "changeIndicator", new PropertyMappingValue(new List<string>() { "ChangeIndicator" }) },
                { "modifiedDate", new PropertyMappingValue(new List<string>() { "ModifiedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _productCategoryPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "CategoryID" }) },
                { "categoryName", new PropertyMappingValue(new List<string>() { "CategoryName" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) },
                { "taxInd", new PropertyMappingValue(new List<string>() { "TaxInd" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _productPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "ProductId" }) },
                { "productName", new PropertyMappingValue(new List<string>() { "ProductName" }) },
                { "quantity", new PropertyMappingValue(new List<string>() { "Quantity" }) },
                { "measurementUnit", new PropertyMappingValue(new List<string>() { "MeasurementUnit.MeasurementName" }) },
                { "purchasePrice", new PropertyMappingValue(new List<string>() { "PurchasePrice" }) },
                { "salesPrice", new PropertyMappingValue(new List<string>() { "SalesPrice" }) },
                { "taxIndStr", new PropertyMappingValue(new List<string>() { "TaxInd" }) },
                { "upcCode", new PropertyMappingValue(new List<string>() { "UpcCode" }) },
                { "upcScanCode", new PropertyMappingValue(new List<string>() { "UpcScanCode" }) },
                { "expireDate", new PropertyMappingValue(new List<string>() { "ExpireDate" }) },
                { "manufactureDate", new PropertyMappingValue(new List<string>() { "ManufactureDate" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) },
                { "supplier", new PropertyMappingValue(new List<string>() { "Supplier.SupplierName" }) },
                { "categoryName", new PropertyMappingValue(new List<string>() { "Category.CategoryName" }) },
                { "itemNo", new PropertyMappingValue(new List<string>() { "ItemNo" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _purchasePropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "PurchaseId" }) },
                { "supplier", new PropertyMappingValue(new List<string>() { "Supplier.SupplierName" }) },
                { "payMethod", new PropertyMappingValue(new List<string>() { "PayMethod" }) },
                { "purchaseDate", new PropertyMappingValue(new List<string>() { "PurchaseDate" }) },
                { "payment", new PropertyMappingValue(new List<string>() { "Payment" }) },
                { "due", new PropertyMappingValue(new List<string>() { "Due" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _purchaseDetailPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "PurchaseDetailId" }) },
                { "purchaseId", new PropertyMappingValue(new List<string>() { "PurchaseId" }) },
                { "product", new PropertyMappingValue(new List<string>() { "Product.ProductName" }) },
                { "measurementUnit", new PropertyMappingValue(new List<string>() { "MeasurementUnit.MeasurementName" }) },
                { "quantity", new PropertyMappingValue(new List<string>() { "Quantity" }) },
                { "price", new PropertyMappingValue(new List<string>() { "Price" }) },
                { "expireDate", new PropertyMappingValue(new List<string>() { "ExpireDate" }) },
                { "manufactureDate", new PropertyMappingValue(new List<string>() { "ManufactureDate" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _purchaseReturnPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "PurchaseReturnId" }) },
                { "purchaseId", new PropertyMappingValue(new List<string>() { "PurchaseId" }) },
                { "product", new PropertyMappingValue(new List<string>() { "Product.ProductName" }) },
                { "quantity", new PropertyMappingValue(new List<string>() { "Quantity" }) },
                { "price", new PropertyMappingValue(new List<string>() { "Price" }) },
                { "discountRate", new PropertyMappingValue(new List<string>() { "DiscountRate" }) },
                { "taxRate", new PropertyMappingValue(new List<string>() { "TaxRate" }) },
                { "returnAmount", new PropertyMappingValue(new List<string>() { "ReturnAmount" }) },
                { "returnType", new PropertyMappingValue(new List<string>() { "ReturnType" }) },
                { "reason", new PropertyMappingValue(new List<string>() { "RejectReason.Reason" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _supplierPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "SupplierId" }) },
                { "supplierName", new PropertyMappingValue(new List<string>() { "SupplierName" }) },
                { "city", new PropertyMappingValue(new List<string>() { "City" }) },
                { "state", new PropertyMappingValue(new List<string>() { "State" }) },
                { "zip", new PropertyMappingValue(new List<string>() { "Zip" }) },
                { "country", new PropertyMappingValue(new List<string>() { "Country" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _roleInfoPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "RoleId" }) },
                { "roleName", new PropertyMappingValue(new List<string>() { "RoleName" }) },
                { "description", new PropertyMappingValue(new List<string>() { "Description" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _userInfoPropertyMapping =
           new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
           {
                { "id", new PropertyMappingValue(new List<string>() { "UserId" }) },
                { "username", new PropertyMappingValue(new List<string>() { "Username" }) },
                { "password", new PropertyMappingValue(new List<string>() { "Password" }) },
                { "displayName", new PropertyMappingValue(new List<string>() { "DisplayName" }) },
                { "fullName", new PropertyMappingValue(new List<string>() { "FullName" }) },
                { "designation", new PropertyMappingValue(new List<string>() { "Designation" }) },
                { "contactNo", new PropertyMappingValue(new List<string>() { "ContactNo" }) },
                { "email", new PropertyMappingValue(new List<string>() { "Email" }) },
                { "address", new PropertyMappingValue(new List<string>() { "Address" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
           };

        private readonly Dictionary<string, PropertyMappingValue> _userRolePropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "UserRoleId" }) },
                { "userName", new PropertyMappingValue(new List<string>() { "User.UserName" }) },
                { "roleName", new PropertyMappingValue(new List<string>() { "Role.RoleName" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _userRoleClaimPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "Id" }) },
                { "roleName", new PropertyMappingValue(new List<string>() { "Role.RoleName" }) },
                { "claimType", new PropertyMappingValue(new List<string>() { "ClaimType" }) },
                { "claimValue", new PropertyMappingValue(new List<string>() { "ClaimValue" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _salesReturnPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "SalesReturnId" }) },
                { "invoiceNo", new PropertyMappingValue(new List<string>() { "InvoiceNo" }) },
                { "product", new PropertyMappingValue(new List<string>() { "Product.ProductName" }) },
                { "quantity", new PropertyMappingValue(new List<string>() { "Quantity" }) },
                { "returnType", new PropertyMappingValue(new List<string>() { "ReturnType" }) },
                { "returnAmount", new PropertyMappingValue(new List<string>() { "ReturnAmount" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _salesMasterPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "SalesId" }) },
                { "paymethod", new PropertyMappingValue(new List<string>() { "PayMethod" }) },
                { "invoiceNo", new PropertyMappingValue(new List<string>() { "InvoiceNo" }) },
                { "salesDate", new PropertyMappingValue(new List<string>() { "SalesDate" }) },
                { "grandTotal", new PropertyMappingValue(new List<string>() { "GrandTotal" }) },
                { "salesTax", new PropertyMappingValue(new List<string>() { "SalesDate" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) },
                { "totalDiscount", new PropertyMappingValue(new List<string>() { "TotalDiscount" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _salesDetailsPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "SalesDetailId" }) },
                { "product", new PropertyMappingValue(new List<string>() { "Product.ProductName" }) },
                { "quantity", new PropertyMappingValue(new List<string>() { "Quantity" }) },
                { "price", new PropertyMappingValue(new List<string>() { "Price" }) },
                { "refPrescriptionId", new PropertyMappingValue(new List<string>() { "RefPrescriptionId" }) },
                { "upcCode", new PropertyMappingValue(new List<string>() { "UpcCode" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _transactionsPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "TransactionId" }) },
                { "payMethod", new PropertyMappingValue(new List<string>() { "PayMethod" }) },
                { "amount", new PropertyMappingValue(new List<string>() { "Amount" }) },
                { "transactionType", new PropertyMappingValue(new List<string>() { "TransactionType" }) },
                { "cardType", new PropertyMappingValue(new List<string>() { "CardType" }) },
                { "checkNo", new PropertyMappingValue(new List<string>() { "CheckNo" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _sourcePropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "SourceID" }) },
                { "fileType", new PropertyMappingValue(new List<string>() { "FileType" }) },
                { "hostAddress", new PropertyMappingValue(new List<string>() { "HostAddress" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _pricerangePropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "PriceRangeId" }) },
                { "productPriceRangeId", new PropertyMappingValue(new List<string>() { "ProductPriceRangeId" }) },
                { "rangeMinRange", new PropertyMappingValue(new List<string>() { "RangeMinRange" }) },
                { "rangeMaxRange", new PropertyMappingValue(new List<string>() { "RangeMaxRange" }) },
                { "markup", new PropertyMappingValue(new List<string>() { "Markup" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _productPricerangePropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "ProductPriceRangeId" }) },
                { "tableName", new PropertyMappingValue(new List<string>() { "TableName" }) },
                { "costPreference", new PropertyMappingValue(new List<string>() { "CostPreference" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _posTerminalPropertyMapping =
           new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
           {
                { "id", new PropertyMappingValue(new List<string>() { "TerminalId" }) },
                { "refNo", new PropertyMappingValue(new List<string>() { "RefNo" }) },
                { "pinpadIpPort", new PropertyMappingValue(new List<string>() { "PinpadIpPort" }) },
                { "pinpadMacAddress", new PropertyMappingValue(new List<string>() { "PinpadMacAddress" }) },
                { "comPort", new PropertyMappingValue(new List<string>() { "ComPort" }) },
                { "ipAddress", new PropertyMappingValue(new List<string>() { "IpAddress" }) },
                { "terminalName", new PropertyMappingValue(new List<string>() { "TerminalName" }) }
           };

        private readonly Dictionary<string, PropertyMappingValue> _customerPropertyMapping =
           new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
           {
                { "id", new PropertyMappingValue(new List<string>() { "CustomerID" }) },
                { "customerName", new PropertyMappingValue(new List<string>() { "CustomerName" }) },
                { "phone", new PropertyMappingValue(new List<string>() { "Phone" }) },
                { "email", new PropertyMappingValue(new List<string>() { "Email" }) },
                { "loyaltyCardNumber", new PropertyMappingValue(new List<string>() { "LoyaltyCardNumber" }) },
                { "loyaltyPointEarned", new PropertyMappingValue(new List<string>() { "LoyaltyPointEarned" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
           };

        private readonly Dictionary<string, PropertyMappingValue> _manufacturerPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "ManufacturerId" }) },
                { "name", new PropertyMappingValue(new List<string>() { "Name" }) },
                { "status", new PropertyMappingValue(new List<string>() { "Status" }) },
                { "createdDate", new PropertyMappingValue(new List<string>() { "CreatedDate" }) }
            };

        private readonly Dictionary<string, PropertyMappingValue> _timesheetPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", new PropertyMappingValue(new List<string>() { "SessionId" }) },
                { "userId", new PropertyMappingValue(new List<string>() { "UserId" }) },
                { "startTime", new PropertyMappingValue(new List<string>() { "StartTime" }) },
                { "endTime", new PropertyMappingValue(new List<string>() { "EndTime" }) },
                { "createdBy", new PropertyMappingValue(new List<string>() { "CreatedBy" }) },
                { "modifiedBy", new PropertyMappingValue(new List<string>() { "ModifiedBy" }) }
            };

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<MeasurementUnitDto, MeasurementUnit>(_measurementUnitPropertyMapping));
            propertyMappings.Add(new PropertyMapping<ManufacturerDto, Manufacturer>(_manufacturerPropertyMapping));

            propertyMappings.Add(new PropertyMapping<ProductCategoryDto, ProductCategory>(_productCategoryPropertyMapping));
            propertyMappings.Add(new PropertyMapping<ProductDto, Product>(_productPropertyMapping));

            propertyMappings.Add(new PropertyMapping<PurchaseMasterDto, PurchaseMaster>(_purchasePropertyMapping));
            propertyMappings.Add(new PropertyMapping<PurchaseDetailDto, PurchaseDetail>(_purchaseDetailPropertyMapping));
            propertyMappings.Add(new PropertyMapping<PurchaseReturnDto, PurchaseReturn>(_purchaseReturnPropertyMapping));

            propertyMappings.Add(new PropertyMapping<EligibleProductDto, EligibleProduct>(_eligibleProductPropertyMapping));

            propertyMappings.Add(new PropertyMapping<SupplierDto, Supplier>(_supplierPropertyMapping));

            propertyMappings.Add(new PropertyMapping<ConfigurationDto, Configuration>(_configurationPropertyMapping));
            propertyMappings.Add(new PropertyMapping<RoleDto, Role>(_roleInfoPropertyMapping));
            propertyMappings.Add(new PropertyMapping<UserDto, User>(_userInfoPropertyMapping));
            propertyMappings.Add(new PropertyMapping<UserRoleDto, UserRole>(_userRolePropertyMapping));
            propertyMappings.Add(new PropertyMapping<RoleClaimDto, RoleClaim>(_userRoleClaimPropertyMapping));

            propertyMappings.Add(new PropertyMapping<SalesMasterDto, SalesMaster>(_salesMasterPropertyMapping));
            propertyMappings.Add(new PropertyMapping<SalesDetailDto, SalesDetail>(_salesDetailsPropertyMapping));
            propertyMappings.Add(new PropertyMapping<SalesReturnDto, SalesReturn>(_salesReturnPropertyMapping));

            propertyMappings.Add(new PropertyMapping<TransactionDto, Transaction>(_transactionsPropertyMapping));

            propertyMappings.Add(new PropertyMapping<SourceDto, Source>(_sourcePropertyMapping));
            propertyMappings.Add(new PropertyMapping<PriceRangeDto, PriceRange>(_pricerangePropertyMapping));

            propertyMappings.Add(new PropertyMapping<ProductPriceRangeDto, ProductPriceRange>(_productPricerangePropertyMapping));
            propertyMappings.Add(new PropertyMapping<PosTerminalDto, PosTerminal>(_posTerminalPropertyMapping));

            propertyMappings.Add(new PropertyMapping<CustomerDto, Customer>(_customerPropertyMapping));

            propertyMappings.Add(new PropertyMapping<SessionDto, Session>(_timesheetPropertyMapping));
        }

        private readonly IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping
            <TSource, TDestination>()
        {
            var matchMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();
            if (matchMapping.Count() == 1)
            {
                return matchMapping.First().MappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            var trimmedField = fields;

            var indexOfFirstSpace = trimmedField.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase);
            var propertyName = indexOfFirstSpace == -1 ?
                trimmedField : trimmedField.Remove(indexOfFirstSpace);

            // find the matching property
            if (!propertyMapping.ContainsKey(propertyName))
            {
                return false;
            }

            return true;
        }
    }
}