using AutoMapper;
using AutoWrapper.Wrappers;
using Contracts;
using Entities.DataModels;
using Entities.Exceptions;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.Handlers;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Service.Logic
{
    internal sealed class CompanyService : ICompanyService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public CompanyService(IRepositoryManager repository, ILoggerManager logger,IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<CompanyDto> companies, MetaData metaData)> GetAllCompaniesAsync(CompanyParameters companyParameters, bool trackChanges)
        {
            var companiesWithMetaData = await _repository.Company.GetAllCompaniesAsync(companyParameters,trackChanges);
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companiesWithMetaData);
            return (companies: companiesDto, metaData: companiesWithMetaData.MetaData);
        }
        public async Task<IEnumerable<CompanyDto>> GetByIdsAsync(IEnumerable<int> ids, bool trackChanges)
        {
            if (ids is null)
                throw new ApiException(new GenericError($"Parameter ids is null", "Parameters NULL", Guid.NewGuid(), DateTime.Now, false), 400);     
            var companyEntities = await _repository.Company.GetByIdsAsync(ids,trackChanges);
            if (ids.Count() != companyEntities.Count())
                throw new ApiException(new GenericError($"Collection count mismatch comparing to ids.", "GET Collection", Guid.NewGuid(), DateTime.Now, false), 400);
            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            return companiesToReturn;
        }
        public async Task<CompanyDto> GetCompanyAsync(int id, bool trackChanges)
        {
            var company = await GetCompanyAndCheckIfItExists(id, trackChanges);
            var companyDto = _mapper.Map<CompanyDto>(company);
            return companyDto;
        }
        public async Task<CompanyDto> CreateCompanyAsync(CompanyForCreationDto company)
        {
           var companyEntity = _mapper.Map<Company>(company);
            _repository.Company.CreateCompany(companyEntity);
            await _repository.SaveAsync();
            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);
            return companyToReturn;
        }
        public async Task<(IEnumerable<CompanyDto> companies, string ids)> CreateCompanyCollectionAsync (IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if (companyCollection is null) 
                throw new ApiException(new GenericError($"Company collection sent from a client is null.", "NULL Company collection", Guid.NewGuid(), DateTime.Now, false), 400);
            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var company in companyEntities)
            {
                _repository.Company.CreateCompany(company);
            }
            await _repository.SaveAsync();
            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id)); 
            return (companies: companyCollectionToReturn, ids: ids);
        }
        public async Task DeleteCompanyAsync(int companyId, bool trackChanges)
        {
            var company = await GetCompanyAndCheckIfItExists(companyId, trackChanges);
            _repository.Company.DeleteCompany(company);
           await _repository.SaveAsync();
        }
        public async Task UpdateCompanyAsync(int companyId, CompanyForUpdateDto companyForUpdate, bool trackChanges)
        {
            var companyEntity = await GetCompanyAndCheckIfItExists(companyId, trackChanges);
            _mapper.Map(companyForUpdate, companyEntity);
            await _repository.SaveAsync ();
        }
        private async Task<Company> GetCompanyAndCheckIfItExists(int id, bool trackChanges)
        {
            var company = await _repository.Company.GetCompanyAsync(id, trackChanges);
            if (company is null)
                throw new ApiException(new GenericError($"The company with id: {id} doesn't exist in the database.", "NO DATA", Guid.NewGuid(), DateTime.Now, false), 400);
            return company;
        }
    }
}
