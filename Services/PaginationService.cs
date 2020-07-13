using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Judge1.Services
{
    public class PaginatedList<T>
    {
        public int PageIndex { get; }
        public int TotalPages { get; }
        public List<T> Items { get; }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        
        public PaginatedList(int total, int pageIndex, int pageSize, IEnumerable<T> items)
        {
            PageIndex = pageIndex;
            TotalPages = (int) Math.Ceiling(total / (double) pageSize);
            
            Items = new List<T>();
            Items.AddRange(items);
        }
    }
    
    public static class PaginationService
    {
        public static PaginatedList<T> Paginate<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            var total = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(total, pageIndex, pageSize, items);
        }
        
        public static async Task<PaginatedList<T>> PaginateAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            var total = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(total, pageIndex, pageSize, items);
        }
    }
}
