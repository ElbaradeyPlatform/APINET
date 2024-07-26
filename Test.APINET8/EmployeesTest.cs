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
namespace Test.APINET8
{
    public class EmployeesTest 
    {

        private EmployeesController _controller;
        private RepositoryManager _repo;
        private IServiceManager serviceManager;
        private readonly Mock<IMapper> _mapper;
        private IDataShaper<EmployeeDto> _shapper;
        public EmployeesTest()
        {
            _shapper = new DataShaper<EmployeeDto>();  
            _mapper = new Mock<IMapper>();
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var builder = new DbContextOptionsBuilder<RepositoryContext>().UseSqlServer(configuration.GetConnectionString("sqlConnection"), b => b.MigrationsAssembly("APINET8"));
            var context = new RepositoryContext(builder.Options);
            _repo = new RepositoryManager(context);
            var logger = new LoggerManager();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();

            serviceManager = new ServiceManager(_repo, logger, mapper, _shapper);
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
        [InlineData(1, 1, 32, 26)]
        public async Task GetCompanyEmployees_ShouldReturnEmployeeList(int pageNumber, int pageSize, int? maxAge,int? minAge, string searchTerm = "")
        {
            //Arrange
            int companyId = 2;
            var _params = new Shared.RequestFeatures.EmployeeParameters(){ PageSize = pageSize, PageNumber = pageNumber, SearchTerm = string.IsNullOrEmpty(searchTerm) ? null : searchTerm };
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
        }
        #endregion
    }
}