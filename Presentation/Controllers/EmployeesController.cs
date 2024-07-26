using AutoWrapper.Wrappers;
using Contracts;
using Entities.DataModels;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Presentation.ActionFilters;
using Presentation.Handlers;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.Handlers;
using Shared.RequestFeatures;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        [HttpGet]
        public async Task<IActionResult> GetEmployeesForCompany(int companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            var pagedResult = await _service.EmployeeService.GetEmployeesAsync(companyId,employeeParameters, trackChanges: false);
            //_logger.LogInfo($"Something went wrong: {pagedResult.SentDate}", nameof(GetEmployeeForCompany) ,Request.GetDisplayUrl());
            return Ok(new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), pagedResult.employees, pagedResult.metaData, string.Empty));
        }

        [HttpGet("{id:int}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(int companyId, int id)
        {
            var employee =await _service.EmployeeService.GetEmployeeAsync(companyId, id, trackChanges: false);
            return Ok( new  GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), Enumerable.Repeat(employee, 1), new MetaData(), string.Empty));
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(int companyId, [FromBody] EmployeeForCreationDto employee)
        {
            var employeeToReturn = await _service.EmployeeService.CreateEmployeeForCompanyAsync(companyId, employee, trackChanges: false);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), Enumerable.Repeat(employeeToReturn, 1), new MetaData(), string.Empty,201));
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmployeeForCompany(int companyId, int id)
        {
            await _service.EmployeeService.DeleteEmployeeForCompanyAsync(companyId, id, trackChanges:false);
            return Ok(new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), null, null, string.Empty));
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(int companyId, int id,[FromBody] EmployeeForUpdateDto employee)
        {
            await _service.EmployeeService.UpdateEmployeeForCompanyAsync(companyId, id, employee,compTrackChanges: false, empTrackChanges: true);
            return Ok(new GenericResponse(DateTime.Now.ToString("yyyy-MM-dd"), null, null, string.Empty));
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
