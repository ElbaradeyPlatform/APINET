using System.Reflection;
using System.Text;

namespace Repository.Extensions
{
    public static class PagingExtension
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> data, int pageSize, int pageIndex) => data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        public static IQueryable<T> FirstPage<T>(this IQueryable<T> data, int pageSize) => data.Take(pageSize);
        public static IQueryable<T> LastPage<T>(this IQueryable<T> data, int pageSize) => data.Skip((data.Count() / pageSize - 1) * pageSize).Take(pageSize);
        //public static int CountofPages<T>(this IQueryable<T> data, int pageSize)
        //{
        //    var total = data.Count();
        //    return total / pageSize + (total % pageSize > 0 ? 1 : 0);
        //}
    }
}
