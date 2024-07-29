using Entities.DataModels;
using Entities.LinkModels;
using Microsoft.AspNetCore.Http;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface ICompanyLinks
    {
        LinkCollectionWrapper<Entity> TryGenerateLinks(IEnumerable<CompanyDto> companiesDto,string fields,  HttpContext httpContext);
    }
}
