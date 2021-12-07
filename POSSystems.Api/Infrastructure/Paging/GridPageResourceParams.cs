namespace POSSystems.Web.Infrastructure.Paging
{
    public class GridFilter : Filter
    {
        public int Id { get; set; }
    }

    public class GridPageResourceParams : PageResourceParams<GridFilter>
    {
        public int? Id => _filterObj?.Id;
    }
}