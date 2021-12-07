using Humanizer;
using Newtonsoft.Json;
using POSSystems.Core;

namespace POSSystems.Web.Infrastructure.Paging
{
    public class Filter
    {
        public string Q { get; set; }
        public string SearchName { get; set; }
        public string Status { get; set; }
    }

    public class PageResourceParams<T> where T : Filter
    {
        private string _filter = string.Empty;

        protected T _filterObj = default;

        public string Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                if (_filter != null)
                {
                    _filterObj = JsonConvert.DeserializeObject<T>(_filter);
                }
            }
        }

        protected string[] _sorterList = null;
        private string _sort = string.Empty;

        public string Sort
        {
            get => _sort;
            set
            {
                _sort = value;
                if (_sort != null)
                {
                    _sorterList = JsonConvert.DeserializeObject<string[]>(_sort);
                    OrderBy = string.Join(' ', _sorterList);
                }
            }
        }

        private const int maxPageSize = 100;
        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public string OrderBy { get; set; } = "Id";
        public string Range { get; set; }
        public string Q => _filterObj?.Q;
        public string Status => (_filterObj == null || _filterObj.Status == null) ? Statuses.Active.Humanize() : _filterObj.Status;
        public string SearchName => _filterObj?.SearchName;
    }

    public class MasterPageResourceParams : PageResourceParams<Filter>
    {
    }

    public class ProductFilter : Filter
    {
        public string SearchCatName { get; set; }
    }

    public class ProductResourceParams : PageResourceParams<ProductFilter>
    {
        public string SearchCatName => _filterObj?.SearchCatName;
    }
}