using AutoMapper;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Presentation.Controllers;
using Service;
using Service.Contracts;
using Shared.DataTransferObjects;
using Repository;
using LoggerService;
using Microsoft.EntityFrameworkCore;
using Moq;
using Service.Contracts.DataShaping;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Dynamic;
using Entities.Handlers;
using API.Utility;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Entities.LinkModels;
using Entities.DataModels;
using System.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using Entities.ConfigurationModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
namespace Test.API
{


    public class CompaniesTest
    {
        private CompaniesController _controller;
        private RepositoryManager _repo;
        private IServiceManager serviceManager;
        private readonly Mock<IMapper> _mapper;
        private IDataShaper<EmployeeDto> _employeeshapper;
        private IDataShaper<CompanyDto> _companyshapper;
        private readonly  IEmployeeLinks _employeeLinks;
        private readonly ICompanyLinks _companyLinks;
        private readonly Mock<LinkGenerator> _linkGenerator;

        public CompaniesTest()
        {
            _employeeshapper = new DataShaper<EmployeeDto>();
            _companyshapper = new DataShaper<CompanyDto>();
             _linkGenerator = new Mock<LinkGenerator>();
            _employeeLinks = new EmployeeLinks(_linkGenerator.Object, _employeeshapper);
            _companyLinks = new CompanyLinks(_linkGenerator.Object, _companyshapper);
         
            _mapper = new Mock<IMapper>();
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var builder = new DbContextOptionsBuilder<RepositoryContext>().UseSqlServer(configuration.GetConnectionString("sqlConnection"), b => b.MigrationsAssembly("API"));
            var context = new RepositoryContext(builder.Options);
            _repo = new RepositoryManager(context);
            var logger = new LoggerManager();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            serviceManager = new ServiceManager(_repo, logger, mapper, _employeeshapper, _companyshapper,_employeeLinks, _companyLinks/*,null,null*/);
            _controller = new CompaniesController(serviceManager);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
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
            var entities = payload.As<LinkCollectionWrapper<Entity>>();
            var data = payload.As<LinkCollectionWrapper<Entity>>().Entities;

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<LinkCollectionWrapper<Entity>>(entities);
            Assert.NotEmpty(data);
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
            var entities = payload.As<LinkCollectionWrapper<Entity>>();
            var data = payload.As<LinkCollectionWrapper<Entity>>().Entities;

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<LinkCollectionWrapper<Entity>>(entities);
            Assert.NotEmpty(data);
            Assert.True(data.Count() < 51);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 1)]
        [InlineData(5, 1)]
        [InlineData(1, 1, "streamline")]
        [InlineData(1, 5, null, "Id")]
        [InlineData(1, 5, null, "Id desc")]
        [InlineData(1, 5, null, "Id,Name")]
        [InlineData(1, 5, null, "Id desc,Name desc")]
        [InlineData(1, 5, null, "Id,Name desc")]
        [InlineData(1, 5, null, "Id desc,Name")]
        [InlineData(1, 5, null, null, "Id,Name")]
        public async Task GetCompanies_ShouldReturnCompanyList(int pageNumber, int pageSize, string searchTerm = "", string orderBy = "", string fields = "")
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters() { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = string.IsNullOrEmpty(searchTerm) ? null : searchTerm, OrderBy = string.IsNullOrEmpty(orderBy) ? null : orderBy, Fields = string.IsNullOrEmpty(fields) ? null : fields };

            //Act
            var result = await _controller.GetCompanies(_params);
            var response = result.As<OkObjectResult>().Value;
            var payload = response.As<GenericResponse>().Payload;
            var entities = payload.As<LinkCollectionWrapper<Entity>>();
            var data = payload.As<LinkCollectionWrapper<Entity>>().Entities;

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<LinkCollectionWrapper<Entity>>(entities);
            Assert.NotEmpty(data);
           
            if (!string.IsNullOrEmpty(searchTerm))
            {
                Assert.True(data.As<List<Entity>>().All(x => (x.Where(c => c.Key == "Name").FirstOrDefault().Value).ToString().ToLower().Contains(searchTerm.ToLower())));
            }

            if (!string.IsNullOrEmpty(fields))
            {
                Assert.True(((IDictionary<String, object>)data.As<List<Entity>>().FirstOrDefault()).Keys.Select(x => x.ToLower()).Intersect(fields.ToLower().Split(',')).Any());
            }
            var actual = data.As<List<Entity>>();
            if (!string.IsNullOrEmpty(orderBy))
            {
                switch (orderBy)
                {
                    case "Id":
                        Assert.True(data.As<List<Entity>>().OrderBy(x => ((IDictionary<string, object>)x)["Id"]).SequenceEqual(actual));
                        break;
                    case "Id desc":
                        Assert.True(data.As<List<Entity>>().OrderByDescending(x => ((IDictionary<string, object>)x)["Id"]).SequenceEqual(actual));
                        break;
                    case "Id,Name":
                        Assert.True(data.As<List<Entity>>().OrderBy(x => ((IDictionary<string, object>)x)["Id"]).ThenBy(x => ((IDictionary<string, object>)x)["Name"]).SequenceEqual(actual));
                        break;
                    case "Id desc,Name desc":
                        Assert.True(data.As<List<Entity>>().OrderByDescending(x => ((IDictionary<string, object>)x)["Id"]).ThenByDescending(x => ((IDictionary<string, object>)x)["Name"]).SequenceEqual(actual));
                        break;
                    case "Id,Name desc":
                        Assert.True(data.As<List<Entity>>().OrderBy(x => ((IDictionary<string, object>)x)["Id"]).ThenByDescending(x => ((IDictionary<string, object>)x)["Name"]).SequenceEqual(actual));
                        break;
                    case "Id desc,Name":
                        Assert.True(data.As<List<Entity>>().OrderByDescending(x => ((IDictionary<string, object>)x)["Id"]).ThenBy(x => ((IDictionary<string, object>)x)["Name"]).SequenceEqual(actual));
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
            var result = await _controller.CreateCompany(_company);
            var response = result.As<CreatedAtRouteResult>().Value;
            var payload = response.As<GenericResponse>().Payload;
            var entities = payload.As<LinkCollectionWrapper<Entity>>();
            var data = payload.As<LinkCollectionWrapper<Entity>>().Entities;

            //Assert
            Assert.IsType<CreatedAtRouteResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<LinkCollectionWrapper<Entity>>(entities);
            Assert.NotEmpty(data);
            Assert.True(response.As<GenericResponse>().Code == 201);
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
            var result = await _controller.CreateCompany(_company);
            var response = result.As<CreatedAtRouteResult>().Value;
            var payload = response.As<GenericResponse>().Payload;
            var entities = payload.As<LinkCollectionWrapper<Entity>>();
            var data = payload.As<LinkCollectionWrapper<Entity>>().Entities;

            //Assert

            Assert.IsType<CreatedAtRouteResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<LinkCollectionWrapper<Entity>>(entities);
            Assert.NotEmpty(data);
            Assert.True(response.As<GenericResponse>().Code == 201);
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
            var result = await _controller.CreateCompanyCollection(_companies);
            var response = result.As<CreatedAtRouteResult>().Value;
            var payload = response.As<GenericResponse>().Payload;
            var entities = payload.As<LinkCollectionWrapper<Entity>>();
            var data = payload.As<LinkCollectionWrapper<Entity>>().Entities;

            //Assert
            Assert.IsType<CreatedAtRouteResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<LinkCollectionWrapper<Entity>>(entities);
            Assert.NotEmpty(data);
            Assert.True(response.As<GenericResponse>().Code == 201);
        }
        #endregion

        #region Delete Company Test
        [Fact]
        public async Task DeleteCompany_ShouldReturnOk()
        {
            //Arrange
            var _company = new CompanyForCreationDto() { Name = "Create Company Test", Address = "21 Adress", Country = "Egypt", Employees = null };

            //Act
            var result = await _controller.CreateCompany(_company);
            var response = result.As<CreatedAtRouteResult>().Value;
            var payload = response.As<GenericResponse>().Payload;
            var entities = payload.As<LinkCollectionWrapper<Entity>>();
            var data = payload.As<LinkCollectionWrapper<Entity>>().Entities;



            var companyId = (int)data.FirstOrDefault().Where(c=>c.Key=="Id").FirstOrDefault().Value;
            var deleteresult = await _controller.DeleteCompany(companyId);
            var _response = deleteresult.As<OkObjectResult>().Value;

            //Create Assert 
            Assert.IsType<CreatedAtRouteResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<LinkCollectionWrapper<Entity>>(entities);
            Assert.NotEmpty(data);
            Assert.True(response.As<GenericResponse>().Code == 201);


            //Delete Assert
            Assert.IsType<OkObjectResult>(deleteresult);
            Assert.IsType<GenericResponse>(_response);
            Assert.True(_response.As<GenericResponse>().Code == 200);

        }


        #endregion

        #region Update Company Test
        [Fact]
        public async Task UpdateCompany_ShouldReturnOk()
        {
            //Arrange
            var _company = new CompanyForUpdateDto(Name: "Admin_Solutions Ltd Upd", Address: "312 Forest Avenue, BF 923", Country: "USA",
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
            var result = await _controller.UpdateCompany(_companyId, _company);
            var response = result.As<OkObjectResult>().Value;
            var payload = response.As<GenericResponse>().Payload;
            var entities = payload.As<LinkCollectionWrapper<Entity>>();
            var data = payload.As<LinkCollectionWrapper<Entity>>().Entities;


            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<GenericResponse>(response);
            Assert.IsType<LinkCollectionWrapper<Entity>>(entities);
            Assert.NotEmpty(data);
            Assert.True(response.As<GenericResponse>().Code == 200);
        }
        #endregion
    }
}