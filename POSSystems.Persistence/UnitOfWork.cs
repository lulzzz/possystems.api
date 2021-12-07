using Microsoft.EntityFrameworkCore;
using POSSystems.Core;
using POSSystems.Core.Repositories;
using POSSystems.Persistence.Repositories;
using System;

namespace POSSystems.Persistence
{
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _applicationDbContext;

        private IProductCategoryRepository _productCategoryRepository;
        private IProductRepository _productRepository;
        private IPurchaseMasterRepository _purchaseMasterRepository;
        private IPurchaseDetailRepository _purchaseDetailRepository;
        private IPurchaseReturnRepository _purchaseReturnRepository;
        private ISalesMasterRepository _salesMasterRepository;
        private ISalesDetailRepository _salesDetailRepository;
        private ISalesReturnRepository _salesReturnRepository;
        private ISupplierRepository _supplierRepository;
        private IMeasurementUnitRepository _measurementUnitRepository;
        private IEligibleProductRepository _eligibleProductRepository;
        private IConfigurationRepository _configurationRepository;
        private IRoleRepository _roleRepository;
        private IUserRepository _userInfoRepository;
        private IUserRoleRepository _userRoleRepository;
        private IRoleClaimRepository _roleClaimRepository;
        private IReportRepository _reportRepository;
        private ITransactionRepository _transactionRepository;
        private ISourceRepository _sourceRepository;
        private BatchFileRepository _batchFileRepository;
        private PriceCatalogRepository _priceCatalogRepository;
        private IPriceRangeRepository _priceRangeRepository;
        private IProductPriceRangeRepository _productPriceRangeRepository;
        private IPosTerminalRepository _posTerminalRepository;
        private ICustomerRepository _customerRepository;
        private IManufacturerRepository _manufacturerRepository;
        private ISessionRepository _sessionRepository;
        private ICompanyRepository _companyRepository;

        public UnitOfWork(string connectionstring)
        {
            var optionsbuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsbuilder.UseSqlServer(connectionstring);

            _applicationDbContext = new ApplicationDbContext(optionsbuilder.Options, null, null);
        }

        public UnitOfWork(ApplicationDbContext applicationDbContext) => _applicationDbContext = applicationDbContext;

        public IProductCategoryRepository ProductCategoryRepository => _productCategoryRepository ??= new ProductCategoryRepository(_applicationDbContext);

        public IProductRepository ProductRepository => _productRepository = _productRepository ??= new ProductRepository(_applicationDbContext);

        public IPurchaseMasterRepository PurchaseMasterRepository => _purchaseMasterRepository ??= new PurchaseMasterRepository(_applicationDbContext);

        public IPurchaseDetailRepository PurchaseDetailRepository => _purchaseDetailRepository ??= new PurchaseDetailRepository(_applicationDbContext);

        public IPurchaseReturnRepository PurchaseReturnRepository => _purchaseReturnRepository ??= new PurchaseReturnRepository(_applicationDbContext);

        public ISalesMasterRepository SalesMasterRepository => _salesMasterRepository ??= new SalesMasterRepository(_applicationDbContext);

        public ISalesDetailRepository SalesDetailRepository => _salesDetailRepository ??= new SalesDetailRepository(_applicationDbContext);

        public ISalesReturnRepository SalesReturnRepository => _salesReturnRepository ??= new SalesReturnRepository(_applicationDbContext);

        public ISourceRepository SourceRepository => _sourceRepository ??= new SourceRepository(_applicationDbContext);

        public ISupplierRepository SupplierRepository => _supplierRepository ??= new SupplierRepository(_applicationDbContext);

        public IMeasurementUnitRepository MeasurementUnitRepository => _measurementUnitRepository ??= new MeasurementUnitRepository(_applicationDbContext);

        public IEligibleProductRepository EligibleProductRepository => _eligibleProductRepository ??= new EligibleProductRepository(_applicationDbContext);

        public IConfigurationRepository ConfigurationRepository => _configurationRepository ??= new ConfigurationRepository(_applicationDbContext);

        public IRoleRepository RoleRepository => _roleRepository ??= new RoleRepository(_applicationDbContext);

        public IRoleClaimRepository RoleClaimRepository => _roleClaimRepository ??= new RoleClaimRepository(_applicationDbContext);

        public IUserRepository UserRepository => _userInfoRepository ??= new UserRepository(_applicationDbContext);

        public IUserRoleRepository UserRoleRepository => _userRoleRepository ??= new UserRoleRepository(_applicationDbContext);

        public IReportRepository ReportRepository => _reportRepository ??= new ReportRepository(_applicationDbContext);

        public ITransactionRepository TransactionRepository => _transactionRepository ??= new TransactionRepository(_applicationDbContext);

        public IPriceRangeRepository PriceRangeRepository => _priceRangeRepository ??= new PriceRangeRepository(_applicationDbContext);

        public IProductPriceRangeRepository ProductPriceRangeRepository => _productPriceRangeRepository ??= new ProductPriceRangeRepository(_applicationDbContext);

        public IBatchFileRepository BatchFileRepository => _batchFileRepository ??= new BatchFileRepository(_applicationDbContext);

        public IPriceCatalogRepository PriceCatalogRepository => _priceCatalogRepository ??= new PriceCatalogRepository(_applicationDbContext);

        public IPosTerminalRepository PosTerminalRepository => _posTerminalRepository ??= new PosTerminalRepository(_applicationDbContext);

        public ICustomerRepository CustomerRepository => _customerRepository ??= new CustomerRepository(_applicationDbContext);

        public IManufacturerRepository ManufacturerRepository => _manufacturerRepository ??= new ManufacturerRepository(_applicationDbContext);

        public ISessionRepository SessionRepository => _sessionRepository ??= new SessionRepository(_applicationDbContext);
        public ICompanyRepository CompanyRepository => _companyRepository ??= new CompanyRepository(_applicationDbContext);

        public bool Save()
        {
            if (!ChangeTracker())
                return true;

            return _applicationDbContext.SaveChanges() > 0;
        }

        public bool TrySave(out string error)
        {
            error = string.Empty;

            try
            {
                if (!ChangeTracker())
                    return true;

                return _applicationDbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool ChangeTracker()
        {
            bool isChanged = false;
            var entries = _applicationDbContext.ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.State != EntityState.Unchanged)
                {
                    isChanged = true;
                }
            }

            return isChanged;
        }

        public void ToggleTracking()
        {
            if (_applicationDbContext.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                _applicationDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                _applicationDbContext.Database.SetCommandTimeout(10000);
            }
            else
            {
                _applicationDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                _applicationDbContext.Database.SetCommandTimeout(500);
            }
        }

        public void ToggleTimeout(int value = 500)
        {
            _applicationDbContext.Database.SetCommandTimeout(value);
        }

        public void Dispose() => _applicationDbContext.Dispose();
    }
}