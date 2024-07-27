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
using Azure;
using System.Dynamic;
using FluentAssertions.Equivalency;
namespace Test.APINET8
{
    public class CompaniesTest
    {
        private CompaniesController _controller;
        private RepositoryManager _repo;
        private IServiceManager serviceManager;
        private readonly Mock<IMapper> _mapper;
        private IDataShaper<EmployeeDto> _employeeshapper;
        private IDataShaper<CompanyDto> _companyshapper;
        public CompaniesTest()
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
            Assert.IsType<List<ExpandoObject>>(payload);
            Assert.NotEmpty(payload);
            Assert.True(payload.Count() < 51);
        }
       
        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 1)]
        [InlineData(5, 1)]
        [InlineData(1, 1, "streamline")]
        [InlineData(1, 5, null,"Id")]
        [InlineData(1, 5, null, "Id desc")]
        [InlineData(1, 5, null, "Id,Name")]
        [InlineData(1, 5, null, "Id desc,Name desc")]
        [InlineData(1, 5, null, "Id,Name desc")]
        [InlineData(1, 5, null, "Id desc,Name")]
        [InlineData(1, 5, null ,null,"Id,Name")]
        public async Task GetCompanies_ShouldReturnCompanyList(int pageNumber, int pageSize, string searchTerm = "",string orderBy="", string fields = "")
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters() { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = string.IsNullOrEmpty(searchTerm) ? null : searchTerm,OrderBy= string.IsNullOrEmpty(orderBy) ? null:orderBy, Fields = string.IsNullOrEmpty(fields) ? null : fields };

            //Act
            var result = await _controller.GetCompanies(_params);
            var response = result.As<OkObjectResult>().Value;
            var payload = response.As<GenericResponse>().Payload;

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<List<ExpandoObject>>(payload);
            Assert.NotEmpty(payload);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                Assert.True(payload.As<List<ExpandoObject>>().All(x => (x.Where(c => c.Key == "Name").FirstOrDefault().Value).ToString().ToLower().Contains(searchTerm.ToLower())));
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
            var company = response.FirstOrDefault().As<CompanyDto>();

            //Assert
            Assert.IsType<CreatedAtRouteResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.NotEmpty(response);
            Assert.IsType<CompanyDto>(company);
            Assert.True(result.As<GenericResponse>().Code == 201);
        }

        [Fact]
        public async Task CreateCompanyWithEmployee_ShouldReturnOk()
        {
            //Arrange
            var _company = new CompanyForCreationDto()
            {
                Name = "Create Company Test 1234",
                Address = "21 Adress",
                Country = "Egypt",
                Employees = new List<EmployeeForCreationDto>()
                {
                    new EmployeeForCreationDto()
                    {
                        Name= "Joan Dane",
                        Age= 29,
                        Position= "Manager"
                    },
                    new EmployeeForCreationDto()
                    {
                        Name= "Martin Geil",
                        Age = 29,
                        Position = "Administrative"
                    }
                }
            };

            //Act
            var data = await _controller.CreateCompany(_company);
            var result = data.As<CreatedAtRouteResult>().Value;
            var response = result.As<GenericResponse>().Payload;
            var company = response.FirstOrDefault().As<CompanyDto>();

            //Assert
            Assert.IsType<CreatedAtRouteResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.NotEmpty(response);
            Assert.IsType<CompanyDto>(company);
            Assert.True(result.As<GenericResponse>().Code == 201);
        }

        [Fact]
        public async Task CreateCompanyCollection_ShouldReturnOk()
        {
            //Arrange
            var _companies = new List<CompanyForCreationDto>()
            {
               new CompanyForCreationDto() 
               { 
                   Name = "Create Company Test", 
                   Address = "21 Adress", 
                   Country = "Egypt", 
                   Employees = null 
               },
               new CompanyForCreationDto() 
               { 
                   Name = "Create Company Test 2", 
                   Address = "212 Adress", 
                   Country = "Egypt", 
                   Employees = null
               } 
            };

            //Act
            var data = await _controller.CreateCompanyCollection(_companies);
            var result = data.As<CreatedAtRouteResult>().Value;
            var companies = result.As<GenericResponse>().Payload;
            var company = (result.As<GenericResponse>().Payload).FirstOrDefault();

            //Assert
            Assert.IsType<CreatedAtRouteResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.NotEmpty(companies);
            Assert.IsType<List<CompanyDto>>(companies);
            Assert.IsType<CompanyDto>(company);
            Assert.True(result.As<GenericResponse>().Code == 200);
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

        #region Update Company Test
        [Fact]
        public async Task UpdateCompany_ShouldReturnOk()
        {
            //Arrange
            var _company = new CompanyForUpdateDto(Name : "Admin_Solutions Ltd Upd", Address : "312 Forest Avenue, BF 923", Country : "USA", 
                new List<EmployeeForCreationDto>() 
                {
                            new EmployeeForCreationDto()
                            {
                                 Name = "Geil Metain",
                                Age= 23,
                                Position ="Admin"
                            }
                });

            int _companyId = 2;
            //Act
            var data = await _controller.UpdateCompany(_companyId, _company);
            var result = data.As<OkObjectResult>().Value;
            var response = (result.As<GenericResponse>().Payload).FirstOrDefault();

            //Assert
            Assert.IsType<OkObjectResult>(data);
            Assert.IsType<GenericResponse>(result);
            Assert.IsType<CompanyForUpdateDto>(response);
            Assert.True(result.As<GenericResponse>().Code == 200);
        }
        #endregion
    }
}