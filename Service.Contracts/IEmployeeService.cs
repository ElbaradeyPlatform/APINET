using Entities.DataModels;
using Entities.Handlers;
using Entities.LinkModels;
using Shared.DataTransferObjects;


namespace Service.Contracts
{
    public interface IEmployeeService
    {
        Task<GenericResponse> GetEmployeesAsync(int companyId, EmployeeLinkParameters linkParameters, bool trackChanges);
        Task<GenericResponse> GetEmployeeAsync(int companyId, int id, EmployeeLinkParameters linkParameters,  bool trackChanges);
         Task<(GenericResponse response,int id)> CreateEmployeeForCompanyAsync(int companyId, EmployeeForCreationDto employeeForCreation, EmployeeLinkParameters linkParameters, bool trackChanges);
        Task DeleteEmployeeForCompanyAsync(int companyId, int id, bool trackChanges);
        Task<GenericResponse> UpdateEmployeeForCompanyAsync(int companyId, int id, EmployeeForUpdateDto employeeForUpdate, EmployeeLinkParameters linkParameters,bool compTrackChanges, bool empTrackChanges);
        Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync(int companyId, int id, bool compTrackChanges, bool empTrackChanges);
        Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee  employeeEntity);
    }
}
