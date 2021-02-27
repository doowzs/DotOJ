using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Shared.Generics
{
    public class PaginatedList<T>
    {
        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalItems { get; }
        public int TotalPages { get; }
        public List<T> Items { get; }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public PaginatedList(int total, int pageIndex, int pageSize, IEnumerable<T> items)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalItems = total;
            TotalPages = (int) Math.Ceiling(total / (double) pageSize);

            Items = new List<T>();
            Items.AddRange(items);
        }
    }

    public static class PaginationExtension
    {
        public static PaginatedList<T> Paginate<T>
            (this IQueryable<T> source, int pageIndex, int pageSize)
            where T : class
        {
            if (pageIndex <= 0) pageIndex = 1;
            var total = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(total, pageIndex, pageSize, items);
        }

        public static async Task<PaginatedList<T>> PaginateAsync<T>
            (this IQueryable<T> source, int pageIndex, int pageSize)
            where T : class
        {
            if (pageIndex <= 0) pageIndex = 1;
            var total = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(total, pageIndex, pageSize, items);
        }

        public static PaginatedList<TR> Paginate<TE, TR>
            (this IQueryable<TE> source, Expression<Func<TE, TR>> selector, int pageIndex, int pageSize)
            where TE : class where TR : class
        {
            if (pageIndex <= 0) pageIndex = 1;
            var total = source.Count();
            var items = source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToList();
            return new PaginatedList<TR>(total, pageIndex, pageSize, items);
        }

        public static async Task<PaginatedList<TR>> PaginateAsync<TE, TR>
            (this IQueryable<TE> source, Expression<Func<TE, TR>> selector, int pageIndex, int pageSize)
            where TE : class where TR : class
        {
            if (pageIndex <= 0) pageIndex = 1;
            var total = await source.CountAsync();
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync();
            return new PaginatedList<TR>(total, pageIndex, pageSize, items);
        }

        public static PaginatedList<TR> Paginate<TE, TI, TR>(this IQueryable<TE> source,
            Expression<Func<TE, TI>> include, Expression<Func<TE, TR>> selector, int pageIndex, int pageSize)
            where TE : class where TI : class where TR : class
        {
            if (pageIndex <= 0) pageIndex = 1;
            var total = source.Count();
            var items = source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(include)
                .Select(selector)
                .ToList();
            return new PaginatedList<TR>(total, pageIndex, pageSize, items);
        }

        public static async Task<PaginatedList<TR>> PaginateAsync<TE, TI, TR>(this IQueryable<TE> source,
            Expression<Func<TE, TI>> include, Expression<Func<TE, TR>> selector, int pageIndex, int pageSize)
            where TE : class where TI : class where TR : class
        {
            if (pageIndex <= 0) pageIndex = 1;
            var total = await source.CountAsync();
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(include)
                .Select(selector)
                .ToListAsync();
            return new PaginatedList<TR>(total, pageIndex, pageSize, items);
        }
    }
}