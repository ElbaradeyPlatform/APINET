using Contracts;
using Entities.DataModels;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using Shared.RequestFeatures;

namespace Repository.Repositories
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
        {
         
        }
        public async Task<PagedList<Company>> GetAllCompaniesAsync(CompanyParameters companyParameters, bool trackChanges)
        {
            var companies = await FindAll(trackChanges)
                .Search(companyParameters.SearchTerm)
                .Sort(companyParameters.OrderBy)
                .Page(companyParameters.PageSize, companyParameters.PageNumber).ToListAsync();
            //    .Skip((companyParameters.PageNumber - 1) * companyParameters.PageSize)
            //    .Take(companyParameters.PageSize).ToListAsync();
            var count = await FindAll( trackChanges).Search(companyParameters.SearchTerm).CountAsync();
            return new PagedList<Company>(companies, count, companyParameters.PageNumber, companyParameters.PageSize);
        }
        public async Task<Company> GetCompanyAsync(int companyId, bool trackChanges) =>await FindByCondition(c => c.Id.Equals(companyId), trackChanges).SingleOrDefaultAsync();
        public void CreateCompany(Company company) => Create(company);
        public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<int> ids, bool trackChanges) => await FindByCondition(x => ids.Contains(x.Id), trackChanges).ToListAsync();
        public void DeleteCompany(Company company) => Delete(company);
    }
}
