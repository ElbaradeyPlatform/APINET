using Entities.Handlers;
using Entities.LinkModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.ActionFilters;
using Presentation.ModelBinders;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;


namespace Presentation.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController : ControllerBase
    {
        private readonly LinkGenerator _linkGenerator;
        public RootController(LinkGenerator linkGenerator) => _linkGenerator = linkGenerator;

        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot([FromHeader(Name = "Accept")] string mediaType)
        {
            if (mediaType.Contains("application/vnd.ae.apiroot"))
            {
                var list = new List<Link>
                {
                    new Link{Href = _linkGenerator.GetUriByName(HttpContext, nameof(GetRoot), new{}),Rel = "self",Method = "GET"},
                    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "CompaniesOptions", new{}),Rel = "CompaniesOptions",Method = "OPTIONS"},
                    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "GetCompanies", new{}),Rel = "companies",Method = "GET"},
                  //  new Link{Href = _linkGenerator.GetUriByName(HttpContext, "collection", new{}),Rel = "company_collection",Method = "GET"},
                 //   new Link{Href = _linkGenerator.GetUriByName(HttpContext, "CompanyById", new{}),Rel = "company_by_Id",Method = "GET"},
                    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "CreateCompanyCollection", new{}),Rel = "create_company_collection",Method = "POST"},
                    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "CreateCompany", new{}),Rel = "create_company",Method = "POST"},
                    //new Link{Href = _linkGenerator.GetUriByName(HttpContext, "DeleteCompany", new{}),Rel = "delete_company",Method = "DELETE"},
                    //new Link{Href = _linkGenerator.GetUriByName(HttpContext, "UpdateCompany", new{}),Rel = "update_company",Method = "PUT"},

                    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "EmployeeOptions", new{}),Rel = "employee_options",Method = "OPTIONS"},
                    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "GetEmployeesForCompany", new{}),Rel = "employees_for_company",Method = "GET"},
                //    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "GetEmployeeForCompany", new{}),Rel = "employee_for_company",Method = "GET"},
                    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "CreateEmployeeForCompany", new{}),Rel = "create_employee",Method = "POST"},
                //    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "DeleteEmployeeForCompany", new{}),Rel = "delete_employee",Method = "DELETE"},
                //    new Link{Href = _linkGenerator.GetUriByName(HttpContext, "UpdateEmployeeForCompany", new{}),Rel = "update_employee",Method = "PUT"},
               //     new Link{Href = _linkGenerator.GetUriByName(HttpContext, "PartiallyUpdateEmployeeForCompany", new{}),Rel = "partially_update_employee",Method = "Patch"},

                };
                return Ok(list);
            }
            return NoContent();
        }
    }
}