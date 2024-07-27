using Entities.DataModels;
using Repository.Utility;
using System.Linq.Dynamic.Core;

namespace Repository.Extensions
{
    public static class RepositoryCompanyExtensions
    {
        public static IQueryable<Company> Search(this IQueryable<Company> companies,string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return companies;
            var lowerCaseTerm = searchTerm.Trim().ToLower();
            return companies.Where(e => e.Name.ToLower().Contains(lowerCaseTerm));
        }
        public static IQueryable<Company> Sort(this IQueryable<Company> companies, string orderByQueryString)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return companies.OrderBy(e => e.Id);
            var orderQuery = OrderQueryBuilder.CreateOrderQuery<Company>(orderByQueryString);
            if (string.IsNullOrWhiteSpace(orderQuery))
                return companies.OrderBy(e => e.Id);
            return companies.OrderBy(orderQuery);
        }
    }
}
