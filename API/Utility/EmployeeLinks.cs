using Contracts;
using Entities.DataModels;
using Entities.LinkModels;
using Shared.DataTransferObjects;
using Microsoft.Net.Http.Headers;
//using Microsoft.AspNetCore.Http;
//using System.Net.Http;
namespace APINET8.Utility
{
    public class EmployeeLinks : IEmployeeLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        public EmployeeLinks(LinkGenerator linkGenerator, IDataShaper<EmployeeDto> dataShaper)
        {
            _linkGenerator = linkGenerator;
            _dataShaper = dataShaper;
        }

        public LinkCollectionWrapper<Entity> TryGenerateLinks(IEnumerable<EmployeeDto> employeesDto, string fields, int companyId, HttpContext httpContext)
        {
            var shapedEmployees = ShapeData(employeesDto, fields);
            if (ShouldGenerateLinks(httpContext))
                return ReturnLinkdedEmployees(employeesDto, fields, companyId, httpContext,shapedEmployees);
            return ReturnShapedEmployees(shapedEmployees);
        }

        private List<Entity> ShapeData(IEnumerable<EmployeeDto> employeesDto, string fields) => _dataShaper.ShapeData(employeesDto, fields).Select(e => e.Entity).ToList();
        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
            var mediaType = (MediaTypeHeaderValue)httpContext.Items["AcceptHeaderMediaType"];
            if (mediaType == null) return false;
            return mediaType.SubTypeWithoutSuffix.EndsWith("hateoas",StringComparison.InvariantCultureIgnoreCase);
        }
        private LinkCollectionWrapper<Entity> ReturnShapedEmployees(List<Entity> shapedEmployees) => new LinkCollectionWrapper<Entity>(shapedEmployees);// new LinkResponse { ShapedEntities = shapedEmployees };
        private LinkCollectionWrapper<Entity> ReturnLinkdedEmployees(IEnumerable<EmployeeDto> employeesDto, string fields, int companyId, HttpContext httpContext, List<Entity> shapedEmployees)
        {
            var employeeDtoList = employeesDto.ToList();
            for (var index = 0; index < employeeDtoList.Count(); index++)
            {
                var employeeLinks = CreateLinksForEmployee(httpContext, companyId, employeeDtoList[index].Id, fields);
                shapedEmployees[index].Add("Links", employeeLinks);
            }
            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
              var linkedEmployees = CreateLinksForEmployees(httpContext, employeeCollection, fields);
           
            //  return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
            return linkedEmployees;
        }
        private List<Link> CreateLinksForEmployee(HttpContext httpContext, int companyId, int id, string fields = "")
        {
            var links = new List<Link>
            {
            new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeeForCompany",values: new { companyId, id, fields }),"self","GET"),
            new Link(_linkGenerator.GetUriByAction(httpContext,"DeleteEmployeeForCompany", values: new { companyId, id }),"delete_employee","DELETE"),
            new Link(_linkGenerator.GetUriByAction(httpContext,"UpdateEmployeeForCompany", values: new { companyId, id }),"update_employee","PUT"),
            new Link(_linkGenerator.GetUriByAction(httpContext,"PartiallyUpdateEmployeeForCompany", values: new { companyId, id }),"partially_update_employee","PATCH")
            };
            return links;
        }
        private LinkCollectionWrapper<Entity> CreateLinksForEmployees(HttpContext httpContext, LinkCollectionWrapper<Entity> employeesWrapper, string fields = "")
        {
            employeesWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeesForCompany", values: new { fields }), "self", "GET"));
         //   employeesWrapper.HasLinks=true;
            return employeesWrapper;
        }
    }
}
