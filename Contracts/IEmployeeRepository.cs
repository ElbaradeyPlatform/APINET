using Entities.DataModels;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IEmployeeRepository
    {
        Task<PagedList<Employee>> GetEmployeesAsync(int companyId, EmployeeParameters employeeParameters, bool trackChanges);
        Task<Employee> GetEmployeeAsync(int companyId, int id, bool trackChanges);
        void CreateEmployeeForCompany(int companyId, Employee employee);
        void DeleteEmployee(Employee employee);
        
    }
}
