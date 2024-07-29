using Contracts;
using Entities.DataModels;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
        { }
        public async Task<PagedList<Employee>> GetEmployeesAsync(int companyId,EmployeeParameters employeeParameters, bool trackChanges)
        {
            var employees = await FindByCondition(e => e.CompanyId.Equals(companyId) , trackChanges)
                .FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge)
                .Search(employeeParameters.SearchTerm)
                .Sort(employeeParameters.OrderBy)
                .Page(employeeParameters.PageSize, employeeParameters.PageNumber).ToListAsync();
            //.Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
            //.Take(employeeParameters.PageSize).ToListAsync();
            var count = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges).FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge).Search(employeeParameters.SearchTerm).CountAsync();
            return new PagedList<Employee>(employees, count, employeeParameters.PageNumber, employeeParameters.PageSize);
        }

        public async Task<Employee> GetEmployeeAsync(int companyId, int id, bool trackChanges)=> await FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(id), trackChanges).SingleOrDefaultAsync();
          
        public void CreateEmployeeForCompany(int companyId, Employee employee)
        {
            employee.CompanyId = companyId;
            Create(employee);
        }
        public void DeleteEmployee(Employee employee) => Delete(employee);
    }
}
