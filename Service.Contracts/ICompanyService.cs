using Entities.Handlers;
using Entities.LinkModels;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System.Dynamic;

namespace Service.Contracts
{
    public interface ICompanyService
    {
        Task<GenericResponse> GetAllCompaniesAsync(CompanyLinkParameters linkParameters, bool trackChanges);
        Task<GenericResponse> GetCompanyAsync(int companyId, CompanyLinkParameters linkParameters, bool trackChanges);
        Task<GenericResponse> GetByIdsAsync(IEnumerable<int> ids, CompanyLinkParameters linkParameters, bool trackChanges);
        Task<(GenericResponse response, int id)> CreateCompanyAsync(CompanyForCreationDto company, CompanyLinkParameters linkParameters);
        Task<(GenericResponse response, string ids)> CreateCompanyCollectionAsync(IEnumerable<CompanyForCreationDto> companyCollection, CompanyLinkParameters linkParameters);
        Task DeleteCompanyAsync(int companyId, bool trackChanges);
        Task<GenericResponse> UpdateCompanyAsync(int companyid, CompanyForUpdateDto companyForUpdate, CompanyLinkParameters linkParameters, bool trackChanges);
    }
}
