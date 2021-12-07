namespace POSSystems.Web.Infrastructure.Paging
{
    public class PaginationMetadata
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int TotalCount { get; set; }

        public string Response => $"{Start}-{End}/{TotalCount}";
    }
}