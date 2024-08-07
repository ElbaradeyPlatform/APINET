using Asp.Versioning;
using Entities.Handlers;
using Entities.LinkModels;
using Microsoft.AspNetCore.Authorization;
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
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IServiceManager _service;
        public CompaniesController(IServiceManager service) => _service = service;

        /// <summary>
        /// Get Options Header
        /// </summary>
        /// <returns>Header Options</returns>
        [HttpOptions(Name = "CompaniesOptions")]
    
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");
            return Ok();
        }

        /// <summary>
        /// Gets the list of all companies
        /// </summary>
        /// <param name="companyParameters"></param>
        /// <returns>Generic Response</returns>
        /// <response code="200">Returns the Companies</response>
        /// <response code="400">If the item is null</response>
        [HttpGet(Name = "GetCompanies")]
        public async Task<IActionResult> GetCompanies([FromQuery] CompanyParameters companyParameters)
        {
            var linkParams = new CompanyLinkParameters(companyParameters, HttpContext);
            var result = await _service.CompanyService.GetAllCompaniesAsync(linkParams, trackChanges: false);
            return Ok(result);
        }

        /// <summary>
        /// Gets the list of all companies by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>Generic Response</returns>
        /// <response code="200">Returns the Companies</response>
        /// <response code="400">If the list  is null</response>
        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType =typeof(ArrayModelBinder))]IEnumerable<int> ids)
        {
            var linkParams = new CompanyLinkParameters(new CompanyParameters(), HttpContext);
            var result = await _service.CompanyService.GetByIdsAsync(ids, linkParams, trackChanges: false);
            return Ok(result);
        }


        /// <summary>
        /// Get Single Company
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Generic Response</returns>
        /// <response code="200">Returns the item</response>
        /// <response code="400">If the item  is null</response>       
        [HttpGet("{id:int}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompany(int id)
        {
            var linkParams = new CompanyLinkParameters(new CompanyParameters(), HttpContext);
            var result = await _service.CompanyService.GetCompanyAsync(id, linkParams,trackChanges: false);
            return Ok(result);
        }

        /// <summary>
        /// Creates a newly created company
        /// </summary>
        /// <param name="company"></param>
        /// <returns>Generic Response</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        /// <response code="422">If the model is invalid</response>
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {
            var linkParams = new CompanyLinkParameters(new CompanyParameters(), HttpContext);
            var result = await _service.CompanyService.CreateCompanyAsync(company, linkParams);
            return CreatedAtRoute("CompanyById", new { id = result.id }, result.response);
        }


        /// <summary>
        /// Creates a newly created companies
        /// </summary>
        /// <param name="companyCollection"></param>
        /// <returns>Generic Response</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        /// <response code="422">If the model is invalid</response>
        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody]IEnumerable<CompanyForCreationDto> companyCollection)
        {
            var linkParams = new CompanyLinkParameters(new CompanyParameters(), HttpContext);
            var result = await _service.CompanyService.CreateCompanyCollectionAsync(companyCollection,linkParams);
            return CreatedAtRoute("CompanyCollection", new { result.ids }, result.response);
        }

        /// <summary>
        /// return success message 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Generic Response</returns>
        /// <response code="200">sucess message</response>
        /// <response code="400">If the item is null</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            await _service.CompanyService.DeleteCompanyAsync(id, trackChanges: true);
            return Ok(value: new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"),payload: null, null, string.Empty));
        }

        /// <summary>
        /// returns updated company
        /// </summary>
        /// <param name="id">int</param>
        /// <param name="company">CompanyForUpdateDto</param>
        /// <returns>Generic Response</returns>
        /// <response code="201">Returns the  updated item</response>
        /// <response code="400">If the item is null</response>
        /// <response code="422">If the model is invalid</response>
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
