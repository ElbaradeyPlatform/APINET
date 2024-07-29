using AutoMapper;
using AutoWrapper.Wrappers;
using Contracts;
using Entities.DataModels;
using Entities.Handlers;
using Entities.LinkModels;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;

namespace Service.Logic
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        private readonly IEmployeeLinks _employeeLinks;
        public EmployeeService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper, IEmployeeLinks employeeLinks)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
            _employeeLinks = employeeLinks;
        }
        public async Task<GenericResponse> GetEmployeesAsync(int companyId, EmployeeLinkParameters linkParameters, bool trackChanges)
        {
            if (!linkParameters.EmployeeParameters.ValidAgeRange)
                throw new ApiException(new GenericError($"Max age can't be less than min age", "MAX AGE RANGE", Guid.NewGuid(), DateTime.Now, false), 400);
            await CheckIfCompanyExists(companyId, trackChanges);
            var employeesWithMetaData = await _repository.Employee.GetEmployeesAsync(companyId, linkParameters.EmployeeParameters,trackChanges);
            var employeesDto =_mapper.Map<IEnumerable<EmployeeDto>>(employeesWithMetaData);
            var links = _employeeLinks.TryGenerateLinks(employeesDto, linkParameters.EmployeeParameters.Fields, companyId, linkParameters.Context);
            return new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), links, employeesWithMetaData.MetaData, string.Empty);
        }
        public async Task<GenericResponse> GetEmployeeAsync(int companyId, int id, EmployeeLinkParameters linkParameters, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);
            var employee = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);
            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            var employees = Enumerable.Repeat(employeeDto,1);
            var links = _employeeLinks.TryGenerateLinks(employees, linkParameters.EmployeeParameters.Fields, companyId, linkParameters.Context);
            return new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), links, new MetaData(), string.Empty);
            
        }
        public async Task<(GenericResponse response, int id)>  CreateEmployeeForCompanyAsync(int companyId, EmployeeForCreationDto employeeForCreation, EmployeeLinkParameters linkParameters, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);
            var employeeEntity = _mapper.Map<Employee>(employeeForCreation);
            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repository.SaveAsync();
            var employeeDto = _mapper.Map<EmployeeDto>(employeeEntity);
            var employees = Enumerable.Repeat(employeeDto, 1);
            var links = _employeeLinks.TryGenerateLinks(employees, linkParameters.EmployeeParameters.Fields, companyId, linkParameters.Context);
            return (new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), links, new MetaData(), string.Empty, 201), employeeDto.Id);
        }
        public async Task<GenericResponse> UpdateEmployeeForCompanyAsync(int companyId, int id, EmployeeForUpdateDto employeeForUpdate, EmployeeLinkParameters linkParameters, bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);
            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);
            _mapper.Map(employeeForUpdate, employeeEntity);
            await _repository.SaveAsync();
            var employeeDto = _mapper.Map<EmployeeDto>(employeeEntity);
            var employees = Enumerable.Repeat(employeeDto, 1);
            var links = _employeeLinks.TryGenerateLinks(employees, linkParameters.EmployeeParameters.Fields, companyId, linkParameters.Context);
            return (new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), links, new MetaData(), string.Empty));
        }
        public async Task DeleteEmployeeForCompanyAsync(int companyId, int id, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);
            var employeeDb = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);
            _repository.Employee.DeleteEmployee(employeeDb);
            await _repository.SaveAsync();
        }
        public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync(int companyId, int id, bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);
            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);
            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            return (employeeToPatch, employeeEntity);
        }
        public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
        {
            _mapper.Map(employeeToPatch, employeeEntity);
            await _repository.SaveAsync();
        }
        private async Task CheckIfCompanyExists(int companyId, bool trackChanges)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company is null)
                throw new ApiException(new GenericError($"The company with id: {companyId} doesn't exist in the database.", "NO DATA", Guid.NewGuid(), DateTime.Now, false), 400);
        }
        private async Task<Employee> GetEmployeeForCompanyAndCheckIfItExists(int companyId, int id, bool trackChanges)
        {
            var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);
            if (employeeDb is null)
                throw new ApiException(new GenericError($"Employee with id: {id} doesn't exist in the database.", "NO DATA", Guid.NewGuid(), DateTime.Now, false), 400);
            return employeeDb;
        }
    }
}
