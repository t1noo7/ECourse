// Common/Extensions/PagingExtensions.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Demo.Common.Models;

namespace Demo.Common.Extensions
{
    public static class PagingExtensions
    {
        public static Pagination<T> GetPaged<T>(this IEnumerable<T> query, int page, int pageSize = 12)
        {
            var result = new Pagination<T>();
            result.PageIndex = page;
            result.PageSize = pageSize;
            result.TotalRecords = query.Count();
            result.Items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return result;
        }
    }
}
