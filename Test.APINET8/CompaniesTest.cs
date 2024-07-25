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
namespace Test.APINET8
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Company, CompanyDto>()
            .ForMember(c => c.FullAddress,
            opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
            CreateMap<Employee, EmployeeDto>();
            CreateMap<CompanyForCreationDto, Company>();
            CreateMap<EmployeeForCreationDto, Employee>();
            CreateMap<EmployeeForUpdateDto, Employee>();
            CreateMap<CompanyForUpdateDto, Company>();
            CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();
        }
    }
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

        [Fact]
        public async Task GetAllCompanies_ShouldReturnOk()
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters() { PageNumber = 1, PageSize = 100 };
            //Act
            var result = await _controller.GetCompanies(_params);
            var response = ((OkObjectResult)result).Value;
            //Assert
            Assert.IsType<OkObjectResult>(result);

        }
        [Fact]
        public async Task GetAllCompanies_ShouldReturnGenericResponse()
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters() { PageNumber = 1, PageSize = 100 };
            //Act
            var result = await _controller.GetCompanies(_params);
            var response = ((OkObjectResult)result).Value;
            var payload = ((GenericResponse)((OkObjectResult)result).Value).Payload;
            //Assert
            Assert.IsType<OkObjectResult>(result);

        }
        [Fact]
        public async Task GetAllCompanies_ShouldReturnCompanyDtoList()
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters() { PageNumber = 1, PageSize = 100 };
            //Act
            var result = await _controller.GetCompanies(_params);
            var response = ((OkObjectResult)result).Value;
            var payload = ((GenericResponse)((OkObjectResult)result).Value).Payload;
            //Assert
            Assert.IsType<GenericResponse>(response);
        }
        [Fact]
        public async Task GetAllCompanies_ShouldReturnNoEmptyList()
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters() { PageNumber = 1, PageSize = 100 };
            //Act
            var result = await _controller.GetCompanies(_params);
            var response = ((OkObjectResult)result).Value;
            var payload = ((GenericResponse)((OkObjectResult)result).Value).Payload;
            //Assert
            Assert.NotEmpty(payload);
        }
        [Fact]
        public async Task GetAllCompanies_ShouldReturnNoMoreThan50CompaniesPerPage()
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters() { PageNumber = 1, PageSize = 100 };
            //Act
            var result = await _controller.GetCompanies(_params);
            var response = ((OkObjectResult)result).Value;
            var payload = ((GenericResponse)((OkObjectResult)result).Value).Payload;
            //Assert
            Assert.True(payload.Count() == 50);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 1)]
        [InlineData(5, 1)]
        public async Task GetAllCompanies_ShouldReturnCompanyList(int pageNumber, int pageSize)
        {
            //Arrange
            var _params = new Shared.RequestFeatures.CompanyParameters() { PageNumber = pageNumber, PageSize = pageSize };
            //Act
            var result = await _controller.GetCompanies(_params);
            var response = ((OkObjectResult)result).Value;
            var payload = ((GenericResponse)((OkObjectResult)result).Value).Payload;
            //Assert
            Assert.NotEmpty(payload);
        }
    }
}