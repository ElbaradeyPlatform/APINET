using AutoMapper;
using AutoWrapper.Wrappers;
using Contracts;
using Entities.DataModels;
using Entities.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.Handlers;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Service.Logic
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        public EmployeeService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
        }

        public async Task<(IEnumerable<ExpandoObject> employees, MetaData metaData)> GetEmployeesAsync(int companyId, EmployeeParameters employeeParameters, bool trackChanges)
        {
            if (!employeeParameters.ValidAgeRange)
                throw new ApiException(new GenericError($"Max age can't be less than min age", "MAX AGE RANGE", Guid.NewGuid(), DateTime.Now, false), 400);
            await CheckIfCompanyExists(companyId, trackChanges);
            var employeesWithMetaData = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesWithMetaData);
            var shapedData = _dataShaper.ShapeData(employeesDto,employeeParameters.Fields);
            return (employees: shapedData, metaData: employeesWithMetaData.MetaData);
       
        }

        public async Task<EmployeeDto> GetEmployeeAsync(int companyId, int id, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);
            var employeeDb = GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);
            var employee = _mapper.Map<EmployeeDto>(employeeDb);
            return employee;
        }
        public async Task<EmployeeDto> CreateEmployeeForCompanyAsync(int companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);
            var employeeEntity = _mapper.Map<Employee>(employeeForCreation);
            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repository.SaveAsync();
            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
            return employeeToReturn;
        }
        public async Task DeleteEmployeeForCompanyAsync(int companyId, int id, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);
            var employeeDb = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);
            _repository.Employee.DeleteEmployee(employeeDb);
            await _repository.SaveAsync();
        }
        public async Task UpdateEmployeeForCompanyAsync(int companyId, int id, EmployeeForUpdateDto employeeForUpdate,bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);
            var employeeDb = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id,empTrackChanges);
            _mapper.Map(employeeForUpdate, employeeDb);
            await _repository.SaveAsync();
        }
        public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync(int companyId, int id, bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);
            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id,empTrackChanges);
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
            var company = await _repository.Company.GetCompanyAsync(companyId,trackChanges);
            if (company is null) 
            throw new ApiException(new GenericError($"The company with id: {companyId} doesn't exist in the database.", "NO DATA", Guid.NewGuid(), DateTime.Now, false), 400);
        }
        private async Task<Employee> GetEmployeeForCompanyAndCheckIfItExists (int companyId, int id, bool trackChanges)
        {
            var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id,trackChanges);
            if (employeeDb is null)
                throw new ApiException(new GenericError($"Employee with id: {id} doesn't exist in the database.", "NO DATA", Guid.NewGuid(), DateTime.Now, false), 400);
            return employeeDb;
        }
    }
}
