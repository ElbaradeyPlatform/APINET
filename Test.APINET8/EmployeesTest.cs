using AutoMapper;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Presentation.Controllers;
using Service;
using Service.Contracts;
using Shared.DataTransferObjects;
using Repository;
using LoggerService;
using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using Moq;
using Entities.DataModels;
using Repository.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using Service.Contracts.DataShaping;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Shared.Handlers;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Builder;
using Xunit;
using System.Dynamic;
using AutoWrapper.Wrappers;
using Microsoft.VisualBasic;
namespace Test.APINET8
{
    public class EmployeesTest 
    {

        private EmployeesController _controller;
        private RepositoryManager _repo;
        private IServiceManager serviceManager;
        private readonly Mock<IMapper> _mapper;
        private IDataShaper<EmployeeDto> _employeeshapper;
        private IDataShaper<CompanyDto> _companyshapper;
        public EmployeesTest()
        {
            _employeeshapper = new DataShaper<EmployeeDto>();
            _companyshapper = new DataShaper<CompanyDto>();
            _mapper = new Mock<IMapper>();
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var builder = new DbContextOptionsBuilder<RepositoryContext>().UseSqlServer(configuration.GetConnectionString("sqlConnection"), b => b.MigrationsAssembly("APINET8"));
            var context = new RepositoryContext(builder.Options);
            _repo = new RepositoryManager(context);
            var logger = new LoggerManager();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();

            serviceManager = new ServiceManager(_repo, logger, mapper, _employeeshapper, _companyshapper);
            _controller = new EmployeesController(serviceManager);

        }
 
        #region Get Employee Test 
        [Fact]
        public async Task GetEmployee_ShouldReturnOk()
        {
            //Arrange
            int companyId = 2;
            int employeeId = 13;

            //Act
            var result = await _controller.GetEmployeeForCompany(companyId, employeeId);
            var response = result.As<OkObjectResult>().Value;
            var payload = response.As<GenericResponse>().Payload;
            var company = payload.As<IEnumerable<EmployeeDto>>().FirstOrDefault();

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.NotEmpty(payload);
            Assert.IsType<EmployeeDto>(company);
        }
        #endregion

        #region Get Employees Test
        [Fact]
        public async Task GetCompanyEmployees_ShouldReturnOk()
        {
            //Arrange
            int companyId = 2;
            var _params = new Shared.RequestFeatures.EmployeeParameters();

            //Act
            var result = await _controller.GetEmployeesForCompany(companyId,_params);
            var response = result.As<OkObjectResult>().Value;
            var payload = response.As<GenericResponse>().Payload;

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<List<ExpandoObject>>(payload);
            Assert.NotEmpty(payload);
            Assert.True(payload.Count() < 51);
        }

        [Theory]
        [InlineData(1, 1,null,null)]
        [InlineData(2, 1,null,null)]
        [InlineData(3, 1,null,null)]
        [InlineData(5, 1,null, null)]
        [InlineData(1, 1, null,null,"Mihael Fins")]
        [InlineData(1, 1, null, null,"ae")]
        [InlineData(1, 1, 32,null)]
        [InlineData(1, 1, null, 26)]
        [InlineData(1, 4, 32, 26)]
        [InlineData(1, 4, 32, 26,null, "Id")]
        [InlineData(1, 4, 32, 26,null, "Id desc")]
        [InlineData(1, 4, 32, 26,null, "Id,Name")]
        [InlineData(1, 4, 32, 26,null, "Id desc,Name desc")]
        [InlineData(1, 4, 32, 26, null, "Id,Name desc")]
        [InlineData(1, 4, 32, 26, null, "Id desc,Name")]
        [InlineData(1, 4, 32, 26, null, null,"name,age")]
        public async Task GetCompanyEmployees_ShouldReturnEmployeeList(int pageNumber, int pageSize, int? maxAge,int? minAge, string searchTerm = "",string orderBy="",string fields="")
        {
            //Arrange
            int companyId = 2;
            var _params = new Shared.RequestFeatures.EmployeeParameters(){ PageSize = pageSize, PageNumber = pageNumber, SearchTerm = string.IsNullOrEmpty(searchTerm) ? null : searchTerm,OrderBy = string.IsNullOrEmpty(orderBy) ? null : orderBy, Fields = string.IsNullOrEmpty(fields) ? null : fields };
            if(maxAge.HasValue)
                _params.MaxAge = maxAge.Value;
            if(minAge.HasValue)
                _params.MinAge = minAge.Value;

            //Act
            var result = await _controller.GetEmployeesForCompany(companyId,_params);
            var response = result.As<OkObjectResult>().Value;
            var payload = response.As<GenericResponse>().Payload;

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<List<ExpandoObject>>(payload);
            Assert.NotEmpty(payload);

            if(!string.IsNullOrEmpty(searchTerm))
            {
                Assert.True(payload.As<List<ExpandoObject>>().All(x => (x.Where(c=>c.Key=="Name").FirstOrDefault().Value).ToString().ToLower().Contains(searchTerm.ToLower())));
            }
        
            if (!string.IsNullOrEmpty(fields))
            {
                Assert.True(((IDictionary<String, object>)payload.As<List<ExpandoObject>>().FirstOrDefault()).Keys.Select(x => x.ToLower()).Intersect(fields.ToLower().Split(',')).Any());
            }
            var actual = payload.As<List<ExpandoObject>>();
            if (!string.IsNullOrEmpty(orderBy))
            {
                switch (orderBy)
                {
                    case "Id":
                        Assert.True(payload.As<List<ExpandoObject>>().OrderBy(x => ((IDictionary<string, object>)x)["Id"]).SequenceEqual(actual));
                        break;
                    case "Id desc":
                        Assert.True(payload.As<List<ExpandoObject>>().OrderByDescending(x => ((IDictionary<string, object>)x)["Id"]).SequenceEqual(actual));
                        break;
                    case "Id,Name":
                        Assert.True(payload.As<List<ExpandoObject>>().OrderBy(x => ((IDictionary<string, object>)x)["Id"]).ThenBy(x => ((IDictionary<string, object>)x)["Name"]).SequenceEqual(actual));
                        break;
                    case "Id desc,Name desc":
                        Assert.True(payload.As<List<ExpandoObject>>().OrderByDescending(x => ((IDictionary<string, object>)x)["Id"]).ThenByDescending(x => ((IDictionary<string, object>)x)["Name"]).SequenceEqual(actual));
                        break;
                    case "Id,Name desc":
                        Assert.True(payload.As<List<ExpandoObject>>().OrderBy(x => ((IDictionary<string, object>)x)["Id"]).ThenByDescending(x => ((IDictionary<string, object>)x)["Name"]).SequenceEqual(actual));
                        break;
                    case "Id desc,Name":
                        Assert.True(payload.As<List<ExpandoObject>>().OrderByDescending(x => ((IDictionary<string, object>)x)["Id"]).ThenBy(x => ((IDictionary<string, object>)x)["Name"]).SequenceEqual(actual));
                        break;
                    default:
                        break;
                }
            }
        }


        #endregion

        #region Create Employee Test
        [Fact]
        public async Task CreateEmployee_ShouldReturnOk()
        {
            //Arrange
            var _employee = new EmployeeForCreationDto() { Name = "Create Employee Test", Age=34 , Position="Manager" };
            int _companyId = 2;

            //Act
            var data = await _controller.CreateEmployeeForCompany(_companyId, _employee);
            var result = data.As<CreatedAtRouteResult>().Value;
            var response = (result.As<GenericResponse>().Payload).FirstOrDefault();

            //Assert
            Assert.IsType<CreatedAtRouteResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.IsType<EmployeeDto>(response);
            Assert.True(result.As<GenericResponse>().Code == 201);
        }
        #endregion

        #region Delete Employee Test
        [Fact]
        public async Task DeleteEmployee_ShouldReturnOk()
        {
            //Arrange
            var _employee = new EmployeeForCreationDto() { Name = "Create Employee Test", Age = 34, Position = "Manager" };
            int _companyId = 2;

            //Act
            //Create
            var data = await _controller.CreateEmployeeForCompany(_companyId, _employee);
            var result = data.As<CreatedAtRouteResult>().Value;
            var response = (result.As<GenericResponse>().Payload).FirstOrDefault();
            var employeeId = response.As<EmployeeDto>().Id;
            //Delete
            var _data = await _controller.DeleteEmployeeForCompany(_companyId,employeeId);
            var _response = _data.As<OkObjectResult>().Value;

            //Assert
            //Create
            Assert.IsType<CreatedAtRouteResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.IsType<EmployeeDto>(response);
            Assert.True(result.As<GenericResponse>().Code == 201);
            //Delete
            Assert.IsType<OkObjectResult>(_data);
            Assert.True(_response.As<GenericResponse>().Code == 200);
        }


        #endregion

        #region Update Employee Test
        [Fact]
        public async Task UpdateEmployee_ShouldReturnOk()
        {
            //Arrange
            var _employee = new EmployeeForUpdateDto() 
            {
                Name = "Sam Raiden",
	            Age = 25,
	            Position= "Software developer",          
            };
            int _companyId = 2;
            int _employeeId = 32;
            //Act
            var data = await _controller.UpdateEmployeeForCompany(_companyId,_employeeId, _employee);
            var result = data.As<OkObjectResult>().Value;
            var response = (result.As<GenericResponse>().Payload).FirstOrDefault();

            //Assert
            Assert.IsType<OkObjectResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.IsType<EmployeeForUpdateDto>(response);
            Assert.True(result.As<GenericResponse>().Code == 200);
        }
        #endregion
    }
}