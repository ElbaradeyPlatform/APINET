using Entities.DataModels;
using Shared.DataTransferObjects;
using Shared.Handlers;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts
{
    public interface IEmployeeService
    {
        Task<(IEnumerable<ExpandoObject> employees, MetaData metaData)> GetEmployeesAsync(int companyId, EmployeeParameters employeeParameters, bool trackChanges);
        Task<EmployeeDto> GetEmployeeAsync(int companyId, int id, bool trackChanges);
         Task<EmployeeDto> CreateEmployeeForCompanyAsync(int companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges);
        Task DeleteEmployeeForCompanyAsync(int companyId, int id, bool trackChanges);
        Task UpdateEmployeeForCompanyAsync(int companyId, int id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChanges);
        Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync(int companyId, int id, bool compTrackChanges, bool empTrackChanges);
        Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee  employeeEntity);
    }
}
