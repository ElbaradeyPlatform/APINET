using AutoMapper;
using Contracts;
using Entities.ConfigurationModels;
using Entities.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Service.Contracts;
using Service.Logic;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public sealed class ServiceManager : IServiceManager
    {
        private readonly Lazy<ICompanyService> _companyService;
        private readonly Lazy<IEmployeeService> _employeeService;
        private readonly Lazy<IAuthenticationService> _authenticationService;
        public ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger,IMapper mapper, IDataShaper<EmployeeDto> employeedataShaper, IDataShaper<CompanyDto> companydataShaper, IEmployeeLinks employeeLinks, ICompanyLinks companyLinks, UserManager<User> userManager, IOptionsSnapshot<JwtConfiguration> configuration)
       {
            _companyService = new Lazy<ICompanyService>(() => new CompanyService(repositoryManager, logger, mapper, companydataShaper, companyLinks));
            _employeeService = new Lazy<IEmployeeService>(() =>new EmployeeService(repositoryManager, logger, mapper, employeedataShaper, employeeLinks));
            _authenticationService = new Lazy<IAuthenticationService>(() =>new AuthenticationService(logger, mapper, userManager,configuration));
        }
        public ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> employeedataShaper, IDataShaper<CompanyDto> companydataShaper, IEmployeeLinks employeeLinks, ICompanyLinks companyLinks)
        {
            _companyService = new Lazy<ICompanyService>(() => new CompanyService(repositoryManager, logger, mapper, companydataShaper, companyLinks));
            _employeeService = new Lazy<IEmployeeService>(() => new EmployeeService(repositoryManager, logger, mapper, employeedataShaper, employeeLinks));
             
        }

        public ICompanyService CompanyService => _companyService.Value;
        public IEmployeeService EmployeeService => _employeeService.Value;
        public IAuthenticationService AuthenticationService => _authenticationService.Value;

    }
}
