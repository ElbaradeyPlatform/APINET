using Entities.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.ActionFilters;
using Presentation.ModelBinders;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.Handlers;
using Shared.RequestFeatures;
using System.ComponentModel.Design;
using System.Text.Json;
namespace Presentation.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IServiceManager _service;
        public CompaniesController(IServiceManager service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetCompanies([FromQuery] CompanyParameters companyParameters)
        {
            var pagedResult = await _service.CompanyService.GetAllCompaniesAsync(companyParameters,trackChanges: false);
            var response = new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), pagedResult.companies, pagedResult.metaData, "");
            return Ok(response);
        }

        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType =typeof(ArrayModelBinder))]IEnumerable<int> ids)
        {
            var companies = await _service.CompanyService.GetByIdsAsync(ids, trackChanges: false);
            return Ok(new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), companies, new MetaData(), string.Empty));
        }

        [HttpGet("{id:int}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompany(int id)
        {
            var company = await _service.CompanyService.GetCompanyAsync(id, trackChanges: false);
            return Ok(new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), Enumerable.Repeat(company, 1), new MetaData(), string.Empty));
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {
            var createdCompany = await _service.CompanyService.CreateCompanyAsync(company);
            return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), Enumerable.Repeat(createdCompany, 1), new MetaData(), string.Empty,201));
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody]IEnumerable<CompanyForCreationDto> companyCollection)
        {
            var result = await _service.CompanyService.CreateCompanyCollectionAsync(companyCollection);
            return CreatedAtRoute("CompanyCollection", new { result.ids }, new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), result.companies, new MetaData(), string.Empty));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            await _service.CompanyService.DeleteCompanyAsync(id, trackChanges: true);
            return Ok(new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), null, null, string.Empty));
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyForUpdateDto company)
        {
            await _service.CompanyService.UpdateCompanyAsync(id, company, trackChanges:true);
            return Ok(new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), Enumerable.Repeat(company, 1), new MetaData(), string.Empty));
        }
    }
}
