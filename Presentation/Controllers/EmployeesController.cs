using AutoWrapper.Wrappers;
using Entities.LinkModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Presentation.ActionFilters;
using Service.Contracts;
using Shared.DataTransferObjects;
using Entities.Handlers;
using Shared.RequestFeatures;
 


namespace Presentation.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IServiceManager _service;
        public EmployeesController(IServiceManager service) {
            _service = service;
        }

        [HttpOptions(Name ="EmployeeOptions")]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");
            return Ok();
        }

        [HttpGet]
        [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
        public async Task<IActionResult> GetEmployeesForCompany(int companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            
            var linkParams = new EmployeeLinkParameters(employeeParameters, HttpContext);
            var result = await _service.EmployeeService.GetEmployeesAsync(companyId, linkParams, trackChanges: false);
            //_logger.LogInfo($"Something went wrong: {pagedResult.SentDate}", nameof(GetEmployeeForCompany) ,Request.GetDisplayUrl());
            return Ok(result);
        }

        [HttpGet("{id:int}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(int companyId, int id)
        {
            var linkParams = new EmployeeLinkParameters(new EmployeeParameters(), HttpContext);
            var result = await _service.EmployeeService.GetEmployeeAsync(companyId, id, linkParams, trackChanges: false);
            return Ok(result);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(int companyId, [FromBody] EmployeeForCreationDto employee)
        {
            var linkParams = new EmployeeLinkParameters(new EmployeeParameters(), HttpContext);
            var result = await _service.EmployeeService.CreateEmployeeForCompanyAsync(companyId, employee, linkParams,trackChanges: false);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = result.id }, result.response);
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmployeeForCompany(int companyId, int id)
        {
            await _service.EmployeeService.DeleteEmployeeForCompanyAsync(companyId, id, trackChanges:true);
            return Ok(new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), payload: null, null, string.Empty));
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(int companyId, int id,[FromBody] EmployeeForUpdateDto employee)
        {
            var linkParams = new EmployeeLinkParameters(new EmployeeParameters(), HttpContext);
           var result =   await _service.EmployeeService.UpdateEmployeeForCompanyAsync(companyId, id, employee, linkParams,compTrackChanges: false, empTrackChanges: true);
            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(int companyId, int id,[FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            var result = await _service.EmployeeService.GetEmployeeForPatchAsync(companyId, id,compTrackChanges: false,empTrackChanges: true);
            patchDoc.ApplyTo(result.employeeToPatch, ModelState);
            TryValidateModel(result.employeeToPatch);
            if (!ModelState.IsValid)
                throw new ApiException(ModelState.AllErrors(), 422);
            await _service.EmployeeService.SaveChangesForPatchAsync(result.employeeToPatch,result.employeeEntity);
            return Ok(new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), Enumerable.Repeat(result.employeeToPatch, 1), null, string.Empty));
        }
    }
}
