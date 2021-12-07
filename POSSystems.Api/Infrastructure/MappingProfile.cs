using AutoMapper;
using Humanizer;
using POSSystems.Core;
using POSSystems.Core.Dtos;
using POSSystems.Core.Dtos.Company;
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
using POSSystems.Core.Dtos.Role;
using POSSystems.Core.Dtos.RoleClaim;
using POSSystems.Core.Dtos.Sales;
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
using System.Linq;

namespace POSSystems.Web.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EntityBase, EntityBase>()
                .ForMember(x => x.CreatedBy, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.ModifiedBy, opt => opt.Ignore())
                .ForMember(x => x.ModifiedDate, opt => opt.Ignore())
                .ForMember(x => x.Status, opt => opt.Ignore());

            // Add as many of these lines as you need to map your objects
            CreateMap<ProductCategory, ProductCategoryDto>().ForMember(dest => dest.Id,
               opts => opts.MapFrom(src => src.CategoryID));
            CreateMap<CreateProductCategoryDto, ProductCategory>();
            CreateMap<UpdateProductCategoryDto, ProductCategory>();

            CreateMap<Supplier, SupplierDto>().ForMember(dest => dest.Id,
               opts => opts.MapFrom(src => src.SupplierId));
            CreateMap<CreateSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>();

            CreateMap<MeasurementUnit, MeasurementUnitDto>().ForMember(dest => dest.Id,
               opts => opts.MapFrom(src => src.MeasurementId));
            CreateMap<CreateMeasurementUnitDto, MeasurementUnit>();
            CreateMap<UpdateMeasurementUnitDto, MeasurementUnit>();

            CreateMap<Product, Core.Dtos.Product.ProductDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.CategoryName, opts => opts.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.MeasurementUnit, opts => opts.MapFrom(src => src.MeasurementUnit.MeasurementName))
                .ForMember(dest => dest.Supplier, opts => opts.MapFrom(src => src.Supplier.SupplierName))
                .ForMember(dest => dest.TableName, opts => opts.MapFrom(src => src.ProductPriceRangeId.HasValue ? src.ProductPriceRange.TableName : string.Empty));

            CreateMap<Product, Core.Dtos.Sales.ProductDto>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ProductId))
               .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.Category.CategoryName))
               .ForMember(dest => dest.MeasurementUnit, opts => opts.MapFrom(src => src.MeasurementUnit.MeasurementName))
               .ForMember(dest => dest.Supplier, opts => opts.MapFrom(src => src.Supplier.SupplierName))
               .ForMember(dest => dest.Product, opts => opts.MapFrom(src => src.ProductName));

            CreateMap<CreateProductDto, Product>()
               .ForMember(dest => dest.ItemNo, opts => opts.MapFrom(src => src.ItemNo))
               .ForMember(dest => dest.MeasurementId, opts => opts.MapFrom(src => src.MeasurementId))
               .ForMember(dest => dest.PackageSize, opts => opts.MapFrom(src => src.PackageSize))
               .ForMember(dest => dest.Quantity, opts => opts.MapFrom(src => src.Quantity))
               .ForMember(dest => dest.SalesPrice, opts => opts.MapFrom(src => src.SalesPrice))
               .ForMember(dest => dest.Strength, opts => opts.MapFrom(src => src.Strength))
               .ForMember(dest => dest.SupplierId, opts => opts.MapFrom(src => src.SupplierId))
               .ForMember(dest => dest.UpcCode, opts => opts.MapFrom(src => src.UpcCode))
               .ForMember(dest => dest.UpcScanCode, opts => opts.MapFrom(src => src.UpcScanCode))
               .ForMember(dest => dest.TaxInd, opts => opts.MapFrom(src => src.TaxInd));

            CreateMap<UpdateProductDto, Product>();

            CreateMap<PurchaseMaster, PurchaseMasterDto>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.PurchaseId))
               .ForMember(dest => dest.Supplier, opts => opts.MapFrom(src => src.Supplier.SupplierName));
            CreateMap<CreatePurchaseMasterDto, PurchaseMaster>();
            CreateMap<UpdatePurchaseMasterDto, PurchaseMaster>();

            CreateMap<PurchaseDetail, PurchaseDetailDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.PurchaseDetailId))
                .ForMember(dest => dest.Product, opts => opts.MapFrom(src => src.Product.ProductName));
            CreateMap<CreatePurchaseDetailDto, PurchaseDetail>();
            CreateMap<UpdatePurchaseDetailDto, PurchaseDetail>();

            CreateMap<PurchaseReturn, PurchaseReturnDto>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.PurchaseReturnId))
               .ForMember(dest => dest.Product, opts => opts.MapFrom(src => src.ProductDetail.ProductName));
            CreateMap<CreatePurchaseReturnDto, PurchaseReturn>();
            CreateMap<UpdatePurchaseReturnDto, PurchaseReturn>();

            CreateMap<EligibleProduct, EligibleProductDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.UPC));
            CreateMap<EligibleProductCSVDto, EligibleProduct>();
            CreateMap<EligibleProduct, EligibleProduct>().IncludeBase<EntityBase, EntityBase>();

            CreateMap<Configuration, ConfigurationDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id));
            CreateMap<CreateConfigurationDto, Configuration>();
            CreateMap<UpdateConfigurationDto, Configuration>();

            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.RoleId));
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.UserId));
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            CreateMap<UserRole, UserRoleDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.UserRoleId))
                .ForMember(dest => dest.UserName, opts => opts.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.RoleName, opts => opts.MapFrom(src => src.Role.RoleName));
            CreateMap<CreateUserRoleDto, UserRole>();
            CreateMap<UpdateUserRoleDto, UserRole>();

            CreateMap<RoleClaim, RoleClaimDto>()
                .ForMember(dest => dest.RoleName, opts => opts.MapFrom(src => src.Role.RoleName));
            CreateMap<CreateRoleClaimDto, RoleClaim>();
            CreateMap<UpdateRoleClaimDto, RoleClaim>();
            CreateMap<string, ClaimDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src))
                .ForMember(dest => dest.Value, opts => opts.MapFrom(src => src));

            CreateMap<CreateSalesDto, BatchPickupPostDataDto>()
                .ForMember(dest => dest.RxList, opts => opts.MapFrom(src => src.RxList.Select(i => i.RxRefillNo.ToString())));

            CreateMap<Product, ProductPurchaseDetailDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.MeasurementUnit, opts => opts.MapFrom(src => src.MeasurementUnit.MeasurementName))
                .ForMember(dest => dest.Supplier, opts => opts.MapFrom(src => src.Supplier.SupplierName));

            CreateMap<Product, ReorderPendingProductDto>();

            CreateMap<SalesReturn, SalesReturnDto>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.SalesReturnId))
               .ForMember(dest => dest.Product, opts => opts.MapFrom(src => src.Product.ProductName));
            CreateMap<CreateSalesReturnDto, SalesReturn>();
            CreateMap<UpdateSalesReturnDto, SalesReturn>();

            CreateMap<SalesMaster, SalesMasterDto>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.SalesId));

            CreateMap<Transaction, TransactionDto>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.TransactionId));

            CreateMap<SalesDetail, SalesDetailDto>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.SalesDetailId))
               .ForMember(dest => dest.Product, opts => opts.MapFrom(src => src.Product.ProductName));

            CreateMap<RxItemDto, RxItemDto>();

            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.TransactionId))
                .ForMember(dest => dest.TransactionType, opts => opts.MapFrom(tt => Converter(tt)));

            CreateMap<Source, SourceDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.SourceID))
                .ForMember(dest => dest.SupplierName, opts => opts.MapFrom(src => src.Supplier.SupplierName));
            CreateMap<CreateSourceDto, Source>();
            CreateMap<UpdateSourceDto, Source>();

            CreateMap<PriceRange, PriceRangeDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.PriceRangeId))
                .ForMember(dest => dest.TableName, opts => opts.MapFrom(src => src.ProductPriceRange.TableName));
            CreateMap<CreatePriceRangeDto, PriceRange>();
            CreateMap<UpdatePriceRangeDto, PriceRange>()
                 .ForMember(dest => dest.PriceRangeId, opts => opts.MapFrom(src => src.Id));

            CreateMap<PurchaseMaster, DeliveryPendingProductDto>();

            CreateMap<ProductPriceRange, ProductPriceRangeDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ProductPriceRangeId))
                .ForMember(dest => dest.CostPreference, opts => opts.MapFrom(src => src.CostPreference.DehumanizeTo<CostPreference>().ToString()));
            CreateMap<ProductPriceRange, GetProductPriceRangeDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ProductPriceRangeId));
            CreateMap<CreateProductPriceRangeDto, ProductPriceRange>()
                .ForMember(dest => dest.CostPreference, opts => opts.MapFrom(src => src.CostPreference));
            CreateMap<UpdateProductPriceRangeDto, ProductPriceRange>()
                 .ForMember(dest => dest.ProductPriceRangeId, opts => opts.MapFrom(src => src.Id));

            CreateMap<PosTerminal, PosTerminalDto>().ForMember(dest => dest.Id,
               opts => opts.MapFrom(src => src.TerminalId));
            CreateMap<CreatePosTerminalDto, PosTerminal>();
            CreateMap<UpdatePosTerminalDto, PosTerminal>();

            CreateMap<ProductCSVDto, PriceCatalog>()
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.CATEGORY.Trim()))
                .ForMember(dest => dest.ProductName, opts => opts.MapFrom(src => src.DESCRIPTION.Trim()))
                .ForMember(dest => dest.ItemNo, opts => opts.MapFrom(src => src.ITEM_NUMBER.Trim()))
                .ForMember(dest => dest.PurchasePrice, opts => opts.MapFrom(src => src.COST_PRICE.Replace('$', ' ').Trim()))
                .ForMember(dest => dest.UPCCode, opts => opts.MapFrom(src => src.UPC.Trim()))
                .ForMember(dest => dest.ScanCode, opts => opts.MapFrom(src => src.UPC.Trim()))
                .ForMember(dest => dest.SalesPrice, opts => opts.MapFrom(src => src.RETAIL_PRICE.Replace('$', ' ').Trim()))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom<CustomResolver, string>(src => src.QTY));

            CreateMap<ProductCSVDto2, PriceCatalog>()
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.category_description.Trim()))
                .ForMember(dest => dest.ProductName, opts => opts.MapFrom(src => src.product_name.Trim()))
                .ForMember(dest => dest.ItemNo, opts => opts.MapFrom(src => src.cndcfi.Trim()))
                .ForMember(dest => dest.SalesPrice, opts => opts.MapFrom(src => src.sales_price.Trim()))
                .ForMember(dest => dest.UPCCode, opts => opts.MapFrom(src => src.upc_code.Trim()))
                .ForMember(dest => dest.Form, opts => opts.MapFrom(src => src.form.Trim()))
                .ForMember(dest => dest.Strength, opts => opts.MapFrom(src => src.strength.Trim()))
                .ForMember(dest => dest.Unit, opts => opts.MapFrom(src => src.measurement_name.Trim()))
                .ForMember(dest => dest.Manufacturer, opts => opts.MapFrom(src => src.manufacture_name.Trim()))
                .ForMember(dest => dest.ScanCode, opts => opts.MapFrom(src => src.ScanCode));

            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.CustomerID));
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();

            CreateMap<Manufacturer, ManufacturerDto>().ForMember(dest => dest.Id,
               opts => opts.MapFrom(src => src.ManufacturerId));
            CreateMap<CreateManufacturerDto, Manufacturer>();
            CreateMap<UpdateManufacturerDto, Manufacturer>();

            CreateMap<Session, SessionDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.SessionId))
                .ForMember(dest => dest.Username, opts => opts.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.StartTime, opts => opts.MapFrom<TimezoneResolver, DateTime>(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opts => opts.MapFrom<NullableTimezoneResolver, DateTime?>(src => src.EndTime));
            CreateMap<Session, PunchDto>()
                .ForMember(dest => dest.StartTime, opts => opts.MapFrom<TimezoneResolver, DateTime>(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opts => opts.MapFrom<NullableTimezoneResolver, DateTime?>(src => src.EndTime));
            CreateMap<CreateSessionDto, Session>()
                .ForMember(dest => dest.StartTime, opts => opts.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opts => opts.MapFrom(src => src.EndTime ?? default(DateTime?)));
            CreateMap<UpdateSessionDto, Session>()
                .ForMember(dest => dest.StartTime, opts => opts.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opts => opts.MapFrom(src => src.EndTime ?? default(DateTime?)));

            CreateMap<Company, CompanyDto>();
            CreateMap<CreateCompanyDto, Company>();
            CreateMap<UpdateCompanyDto, Company>();
        }

        public class NullableTimezoneResolver: IMemberValueResolver<object, object, DateTime?, DateTime?>
        {
            private readonly UserCultureInfo _userCultureInfo;
            public NullableTimezoneResolver(UserCultureInfo userCultureInfo)
            {
                _userCultureInfo = userCultureInfo;
            }

            public DateTime? Resolve(object source, object destination, DateTime? sourceMember, DateTime? destMember, ResolutionContext context)
            {
                return _userCultureInfo.GetUserLocalTime(sourceMember);
            }
        }

        public class TimezoneResolver : IMemberValueResolver<object, object, DateTime, DateTime>
        {
            private readonly UserCultureInfo _userCultureInfo;
            public TimezoneResolver(UserCultureInfo userCultureInfo)
            {
                _userCultureInfo = userCultureInfo;
            }

            public DateTime Resolve(object source, object destination, DateTime sourceMember, DateTime destMember, ResolutionContext context)
            {
                return _userCultureInfo.GetUserLocalTime(sourceMember).GetValueOrDefault();
            }
        }

        private class CustomResolver : IMemberValueResolver<object, object, string, int>
        {
            public int Resolve(object source, object destination, string sourceMember, int destMember, ResolutionContext context)
            {
                int.TryParse(sourceMember.Trim(), out int quantity);
                return quantity;
            }
        }

        private static object Converter(Transaction value)
        {
            return Enum.GetName(typeof(TransactionType), value.TransactionType.DehumanizeTo<TransactionType>()).Humanize();
        }
    }
}