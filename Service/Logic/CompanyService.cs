using AutoMapper;
using AutoWrapper.Wrappers;
using Contracts;
using Entities.DataModels;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using Entities.Handlers;
using Entities.LinkModels;
using System.ComponentModel.Design;


namespace Service.Logic
{
    internal sealed class CompanyService : ICompanyService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<CompanyDto> _dataShaper;
        private readonly ICompanyLinks _comanyLinks;
        public CompanyService(IRepositoryManager repository, ILoggerManager logger,IMapper mapper, IDataShaper<CompanyDto> dataShaper ,ICompanyLinks comanyLinks)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
            _comanyLinks = comanyLinks;
        }
        public async Task<GenericResponse> GetAllCompaniesAsync(CompanyLinkParameters linkParameters, bool trackChanges)
        {
            var companiesWithMetaData = await _repository.Company.GetAllCompaniesAsync(linkParameters.CompanyParameters, trackChanges);
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companiesWithMetaData);
            var links = _comanyLinks.TryGenerateLinks(companiesDto, linkParameters.CompanyParameters.Fields, linkParameters.Context);
            return new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), links, companiesWithMetaData.MetaData, string.Empty);
        }
        public async Task<GenericResponse> GetByIdsAsync(IEnumerable<int> ids, CompanyLinkParameters linkParameters, bool trackChanges)
        {
            if (ids is null)
                throw new ApiException(new GenericError($"Parameter ids is null", "Parameters NULL", Guid.NewGuid(), DateTime.Now, false), 400);     
            var companyEntities = await _repository.Company.GetByIdsAsync(ids,trackChanges);
            if (ids.Count() != companyEntities.Count())
                throw new ApiException(new GenericError($"Collection count mismatch comparing to ids.", "GET Collection", Guid.NewGuid(), DateTime.Now, false), 400);
            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var links = _comanyLinks.TryGenerateLinks(companiesToReturn, linkParameters.CompanyParameters.Fields, linkParameters.Context);
            return new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), links, new MetaData(), string.Empty);
        }
        public async Task<GenericResponse> GetCompanyAsync(int id, CompanyLinkParameters linkParameters, bool trackChanges)
        {
            var company = await GetCompanyAndCheckIfItExists(id, trackChanges);
            var companyDto = _mapper.Map<CompanyDto>(company);
            var companies = Enumerable.Repeat(companyDto, 1);
            var links = _comanyLinks.TryGenerateLinks(companies, linkParameters.CompanyParameters.Fields, linkParameters.Context);
            return new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), links, new MetaData(), string.Empty);
        }
        public async Task<(GenericResponse response, int id)> CreateCompanyAsync(CompanyForCreationDto company,CompanyLinkParameters linkParameters )
        {
           var companyEntity = _mapper.Map<Company>(company);
            _repository.Company.CreateCompany(companyEntity);
            await _repository.SaveAsync();
            var companyDto = _mapper.Map<CompanyDto>(companyEntity);
            var companies = Enumerable.Repeat(companyDto, 1);
            var links = _comanyLinks.TryGenerateLinks(companies, linkParameters.CompanyParameters.Fields, linkParameters.Context);
            return (new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"),links, new MetaData(), string.Empty, 201), companyDto.Id);
        }
        public async Task<(GenericResponse response, string ids)> CreateCompanyCollectionAsync (IEnumerable<CompanyForCreationDto> companyCollection,CompanyLinkParameters linkParameters)
        {
            if (companyCollection is null) 
                throw new ApiException(new GenericError($"Company collection sent from a client is null.", "NULL Company collection", Guid.NewGuid(), DateTime.Now, false), 400);
            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var company in companyEntities)
            {
                _repository.Company.CreateCompany(company);
            }
            await _repository.SaveAsync();
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var ids = string.Join(",", companiesDto.Select(c => c.Id));
            var links = _comanyLinks.TryGenerateLinks(companiesDto, linkParameters.CompanyParameters.Fields, linkParameters.Context);
            return (new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), links, new MetaData(), string.Empty, 201), ids);
        }
        public async Task DeleteCompanyAsync(int companyId, bool trackChanges)
        {
            var company = await GetCompanyAndCheckIfItExists(companyId, trackChanges);
            _repository.Company.DeleteCompany(company);
           await _repository.SaveAsync();
        }
        public async Task<GenericResponse> UpdateCompanyAsync(int companyId, CompanyForUpdateDto companyForUpdate, CompanyLinkParameters linkParameters ,bool trackChanges)
        {
            var companyEntity = await GetCompanyAndCheckIfItExists(companyId, trackChanges);
             _mapper.Map(companyForUpdate, companyEntity);         
            await _repository.SaveAsync ();
            var companyDto = _mapper.Map<CompanyDto>(companyEntity);
            var companies = Enumerable.Repeat(companyDto, 1);
            var links = _comanyLinks.TryGenerateLinks(companies, linkParameters.CompanyParameters.Fields, linkParameters.Context);
            return (new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), links, new MetaData(), string.Empty));
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
