using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System.Dynamic;

namespace Service.Contracts
{
        public interface ICompanyService
        {
            Task<(IEnumerable<ExpandoObject> companies, MetaData metaData)> GetAllCompaniesAsync(CompanyParameters companyParameters, bool trackChanges);
            Task<CompanyDto> GetCompanyAsync(int companyId, bool trackChanges);
            Task<CompanyDto> CreateCompanyAsync(CompanyForCreationDto company);
            Task<IEnumerable<CompanyDto>> GetByIdsAsync(IEnumerable<int> ids, bool trackChanges);
            Task<(IEnumerable<CompanyDto> companies, string ids)>CreateCompanyCollectionAsync(IEnumerable<CompanyForCreationDto> companyCollection);
            Task DeleteCompanyAsync(int companyId, bool trackChanges);
            Task UpdateCompanyAsync(int companyid, CompanyForUpdateDto companyForUpdate,bool trackChanges);
        }
}
