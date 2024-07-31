using Asp.Versioning;
using Entities.Handlers;
using Entities.LinkModels;
using Microsoft.AspNetCore.Mvc;
using Presentation.ActionFilters;
using Presentation.ModelBinders;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System.Xml.Linq;


namespace Presentation.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/companies")]
   // [Route("api/{v:apiversion}/companies")]
    [ApiController]
    [ResponseCache(CacheProfileName = "120SecondsDuration")]
    public class CompaniesController : ControllerBase
    {
        private readonly IServiceManager _service;
        public CompaniesController(IServiceManager service) => _service = service;

        [HttpOptions(Name = "CompaniesOptions")]
    
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");
            return Ok();
        }

        [HttpGet(Name = "GetCompanies")]
   //     [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetCompanies([FromQuery] CompanyParameters companyParameters)
        {
            var linkParams = new CompanyLinkParameters(companyParameters, HttpContext);
            var result = await _service.CompanyService.GetAllCompaniesAsync(linkParams, trackChanges: false);
            return Ok(result);
        }

        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType =typeof(ArrayModelBinder))]IEnumerable<int> ids)
        {
            var linkParams = new CompanyLinkParameters(new CompanyParameters(), HttpContext);
            var result = await _service.CompanyService.GetByIdsAsync(ids, linkParams, trackChanges: false);
            return Ok(result);
        }

        [HttpGet("{id:int}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompany(int id)
        {
            var linkParams = new CompanyLinkParameters(new CompanyParameters(), HttpContext);
            var result = await _service.CompanyService.GetCompanyAsync(id, linkParams,trackChanges: false);
            return Ok(result);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {
            var linkParams = new CompanyLinkParameters(new CompanyParameters(), HttpContext);
            var result = await _service.CompanyService.CreateCompanyAsync(company, linkParams);
            return CreatedAtRoute("CompanyById", new { id = result.id }, result.response);
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody]IEnumerable<CompanyForCreationDto> companyCollection)
        {
            var linkParams = new CompanyLinkParameters(new CompanyParameters(), HttpContext);
            var result = await _service.CompanyService.CreateCompanyCollectionAsync(companyCollection,linkParams);
            return CreatedAtRoute("CompanyCollection", new { result.ids }, result.response);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            await _service.CompanyService.DeleteCompanyAsync(id, trackChanges: true);
            return Ok(value: new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"),payload: null, null, string.Empty));
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyForUpdateDto company)
        {
            var linkParams = new CompanyLinkParameters(new CompanyParameters(), HttpContext);
            var result =   await _service.CompanyService.UpdateCompanyAsync(id, company, linkParams, trackChanges:true);
            return Ok(result);
        }
    }
}
