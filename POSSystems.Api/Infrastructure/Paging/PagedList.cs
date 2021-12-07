using System;
using System.Collections.Generic;
using System.Linq;

namespace POSSystems.Web.Infrastructure.Paging
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int Start { get; private set; }
        public int End { get; private set; }

        public bool HasPrevious => (CurrentPage > 1);

        public bool HasNext => (CurrentPage < TotalPages);

        public PagedList(List<T> items, int count, int start, int end)
        {
            TotalCount = count;
            Start = start;
            End = end;
            TotalPages = (int)Math.Ceiling(count / (double)(end - start - 1));
            AddRange(items);
        }

        public static PagedList<T> Create(IQueryable<T> source, string range)
        {
            int start = 0, end;
            var count = source.Count();

            if (range != null)
            {
                var ranges = range.Split(new string[] { "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries);
                start = int.Parse(ranges[0]);
                end = int.Parse(ranges[1]);
            }
            else
            {
                end = count == 0 ? 0 : count - 1;
            }

            var items = source.Skip(start).Take(end - start + 1).ToList();
            return new PagedList<T>(items, count, start, end);
        }

        public static PagedList<T> Create(List<T> source)
        {
            var count = source.Count();
            int start = 0, end = count == 0 ? 0 : count - 1;
            return new PagedList<T>(source, count, start, end);
        }
    }
}