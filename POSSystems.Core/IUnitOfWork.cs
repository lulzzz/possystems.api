using POSSystems.Core.Repositories;

namespace POSSystems.Core
{
    public interface IUnitOfWork
    {
        IProductCategoryRepository ProductCategoryRepository { get; }
        IProductRepository ProductRepository { get; }
        IPurchaseMasterRepository PurchaseMasterRepository { get; }
        IPurchaseDetailRepository PurchaseDetailRepository { get; }
        IPurchaseReturnRepository PurchaseReturnRepository { get; }
        ISalesMasterRepository SalesMasterRepository { get; }
        ISalesDetailRepository SalesDetailRepository { get; }
        ISalesReturnRepository SalesReturnRepository { get; }
        IReportRepository ReportRepository { get; }
        ISupplierRepository SupplierRepository { get; }
        IMeasurementUnitRepository MeasurementUnitRepository { get; }
        IEligibleProductRepository EligibleProductRepository { get; }
        IConfigurationRepository ConfigurationRepository { get; }
        IRoleRepository RoleRepository { get; }
        IUserRepository UserRepository { get; }
        IUserRoleRepository UserRoleRepository { get; }
        IRoleClaimRepository RoleClaimRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        ISourceRepository SourceRepository { get; }
        IPriceCatalogRepository PriceCatalogRepository { get; }
        IBatchFileRepository BatchFileRepository { get; }

        IPriceRangeRepository PriceRangeRepository { get; }
        IProductPriceRangeRepository ProductPriceRangeRepository { get; }
        IPosTerminalRepository PosTerminalRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        IManufacturerRepository ManufacturerRepository { get; }
        ISessionRepository SessionRepository { get; }
        ICompanyRepository CompanyRepository { get; }


        bool Save();

        bool TrySave(out string message);

        bool ChangeTracker();

        void ToggleTracking();

        void ToggleTimeout(int value = 500);
    }
}