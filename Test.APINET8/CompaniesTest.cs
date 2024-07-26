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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace Test.APINET8
{
    public class CompaniesTest
    {
        private CompaniesController _controller;
        private RepositoryManager _repo;
        private IServiceManager serviceManager;
        private readonly Mock<IMapper> _mapper;
        public CompaniesTest()
        {
            _mapper = new Mock<IMapper>();
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var builder = new DbContextOptionsBuilder<RepositoryContext>().UseSqlServer(configuration.GetConnectionString("sqlConnection"), b => b.MigrationsAssembly("APINET8"));
            var context = new RepositoryContext(builder.Options);
            _repo = new RepositoryManager(context);
            var logger = new LoggerManager();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            serviceManager = new ServiceManager(_repo, logger, mapper);
            _controller = new CompaniesController(serviceManager);
        }

        #region Get Company Test 
        [Fact]
        public async Task GetCompany_ShouldReturnOk()
        {
            //Arrange

            //Act
            var result = await _controller.GetCompany(2);
            var response = result.As<OkObjectResult>().Value;
            var payload = response.As<GenericResponse>().Payload;
            var company = payload.As<IEnumerable<CompanyDto>>().FirstOrDefault();

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.NotEmpty(payload);
            Assert.IsType<CompanyDto>(company);
        }
        #endregion

        #region Get Companies Test
        [Fact]
        public async Task GetCompanies_ShouldReturnOk()
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters();

            //Act
            var result = await _controller.GetCompanies(_params);
            var response = result.As<OkObjectResult>().Value;
            var payload = response.As<GenericResponse>().Payload;

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<List<CompanyDto>>(payload);
            Assert.NotEmpty(payload);
            Assert.True(payload.Count() < 51);
        }
       
        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 1)]
        [InlineData(5, 1)]
        [InlineData(1, 1, "streamline")]
        public async Task GetCompanies_ShouldReturnCompanyList(int pageNumber, int pageSize, string searchTerm = "")
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters() { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = string.IsNullOrEmpty(searchTerm) ? null : searchTerm };

            //Act
            var result = await _controller.GetCompanies(_params);
            var response = result.As<OkObjectResult>().Value;
            var payload = response.As<GenericResponse>().Payload;

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<List<CompanyDto>>(payload);
            Assert.NotEmpty(payload);
        }
        #endregion

        #region Create Company Test
        [Fact]
        public async Task CreateCompany_ShouldReturnOk()
        {
            //Arrange
            var _company = new CompanyForCreationDto() { Name = "Create Company Test", Address = "21 Adress", Country = "Egypt", Employees = null };

            //Act
            var data = await _controller.CreateCompany(_company);
            var result = data.As<CreatedAtRouteResult>().Value;
            var response = result.As<GenericResponse>().Payload;

            //Assert
            Assert.IsType<CreatedAtRouteResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.NotEmpty(response);
           Assert.True(result.As<GenericResponse>().Code==201);
        }

        [Fact]
        public async Task CreateCompanyCollection_ShouldReturnOk()
        {
            //Arrange
            var _companies = new List<CompanyForCreationDto>();
            var _company = new CompanyForCreationDto() { Name = "Create Company Test", Address = "21 Adress", Country = "Egypt", Employees = null };
            var _company2 = new CompanyForCreationDto() { Name = "Create Company Test 2", Address = "212 Adress", Country = "Egypt", Employees = null };
            _companies.Add(_company);
            _companies.Add(_company2);

            //Act
            var data = await _controller.CreateCompany(_company);
            var result = data.As<CreatedAtRouteResult>().Value;
            var response = (result.As<GenericResponse>().Payload).FirstOrDefault();

            //Assert
            Assert.IsType<CreatedAtRouteResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.IsType<CompanyDto>(response);
            Assert.True(result.As<GenericResponse>().Code == 201);
        }
        #endregion

        #region Delete Company Test
        [Fact]
        public async Task DeleteCompany_ShouldReturnOk()
        {
            //Arrange
            var _company = new CompanyForCreationDto() { Name = "Create Company Test", Address = "21 Adress", Country = "Egypt", Employees = null };

            //Act
            var data = await _controller.CreateCompany(_company);
            var result = data.As<CreatedAtRouteResult>().Value;
            var response = (result.As<GenericResponse>().Payload).FirstOrDefault();
            var companyId = response.As<CompanyDto>().Id;
            var _data = await _controller.DeleteCompany(companyId);
            var _response = _data.As<OkObjectResult>().Value;

            //Assert
            Assert.IsType<CreatedAtRouteResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.IsType<CompanyDto>(response);
            Assert.True(result.As<GenericResponse>().Code == 201);
            Assert.IsType<OkObjectResult>(_data);
            Assert.True(_response.As<GenericResponse>().Code == 200);

        }


        #endregion
    }
}