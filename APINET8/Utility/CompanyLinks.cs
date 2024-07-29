using Contracts;
using Entities.DataModels;
using Entities.LinkModels;
using Shared.DataTransferObjects;
using Microsoft.Net.Http.Headers;
//using Microsoft.AspNetCore.Http;
//using System.Net.Http;
namespace APINET8.Utility
{
    public class CompanyLinks : ICompanyLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<CompanyDto> _dataShaper;
        public CompanyLinks(LinkGenerator linkGenerator, IDataShaper<CompanyDto> dataShaper)
        {
            _linkGenerator = linkGenerator;
            _dataShaper = dataShaper;
        }

        public LinkCollectionWrapper<Entity> TryGenerateLinks(IEnumerable<CompanyDto> companiesDto, string fields,HttpContext httpContext)
        {
            var shapedEmployees = ShapeData(companiesDto, fields);
            if (ShouldGenerateLinks(httpContext))
                return ReturnLinkdedEmployees(companiesDto, fields, httpContext,shapedEmployees);
            return ReturnShapedEmployees(shapedEmployees);
        }

        private List<Entity> ShapeData(IEnumerable<CompanyDto> companiesDto, string fields) => _dataShaper.ShapeData(companiesDto, fields).Select(e => e.Entity).ToList();
        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
            var mediaType = (MediaTypeHeaderValue)httpContext.Items["AcceptHeaderMediaType"];
            if (mediaType == null) return false;    
            return mediaType.SubTypeWithoutSuffix.EndsWith("hateoas",StringComparison.InvariantCultureIgnoreCase);
        }
        private LinkCollectionWrapper<Entity> ReturnShapedEmployees(List<Entity> shapedCompanies) => new LinkCollectionWrapper<Entity>(shapedCompanies);// new LinkResponse { ShapedEntities = shapedEmployees };
        private LinkCollectionWrapper<Entity> ReturnLinkdedEmployees(IEnumerable<CompanyDto> companiesDto, string fields, HttpContext httpContext, List<Entity> shapedCompanies)
        {
            var companiesDtoList = companiesDto.ToList();
            for (var index = 0; index < companiesDtoList.Count(); index++)
            {
                var companiesLinks = CreateLinksForCompany(httpContext, companiesDtoList[index].Id, fields);
                shapedCompanies[index].Add("Links", companiesLinks);
            }
            var companyCollection = new LinkCollectionWrapper<Entity>(shapedCompanies);
              var linkedCompanies= CreateLinksForCompanies(httpContext, companyCollection, fields);
           
            //  return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
            return linkedCompanies;
        }
        private List<Link> CreateLinksForCompany(HttpContext httpContext,  int id, string fields = "")
        {
            var links = new List<Link>
            {
            new Link(_linkGenerator.GetUriByAction(httpContext, "GetCompany",values: new { id, fields }),"self","GET"),
            new Link(_linkGenerator.GetUriByAction(httpContext,"DeleteCompany", values: new {  id }),"delete_company","DELETE"),
            new Link(_linkGenerator.GetUriByAction(httpContext,"UpdateCompany", values: new {   id }),"update_company","PUT"),

            };
            return links;
        }
        private LinkCollectionWrapper<Entity> CreateLinksForCompanies(HttpContext httpContext, LinkCollectionWrapper<Entity> companiesWrapper, string fields = "")
        {
            companiesWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext, "GetCompanies", values: new { fields }), "self", "GET"));
           // companiesWrapper.HasLinks=true;
            return companiesWrapper;
        }
    }
}
