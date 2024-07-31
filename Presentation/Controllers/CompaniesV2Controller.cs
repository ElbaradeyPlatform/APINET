using Asp.Versioning;
using Entities.LinkModels;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiVersion("2.0", Deprecated = true)]
    [Route("api/companies")]
  //  [Route("api/{v:apiversion}/companies")]
    [ApiController]
    public class CompaniesV2Controller : ControllerBase
    {
        private readonly IServiceManager _service;
        public CompaniesV2Controller(IServiceManager service) => _service = service;

        [HttpGet] 
        public async Task<IActionResult> GetCompanies([FromQuery] CompanyParameters companyParameters)
        {
            var linkParams = new CompanyLinkParameters(companyParameters, HttpContext);
            var result = await _service.CompanyService.GetAllCompaniesAsync(linkParams, trackChanges: false);
            return Ok(result);
        }
    }
}
