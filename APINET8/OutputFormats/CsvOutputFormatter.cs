using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Shared.DataTransferObjects;
using System.Text;


namespace APINET8.OutputFormats
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(CompanyDto).IsAssignableFrom(type) || typeof(IEnumerable<CompanyDto>).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }
            if (typeof(EmployeeDto).IsAssignableFrom(type) || typeof(IEnumerable<EmployeeDto>).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }
            return false;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();
 
            if (context.Object is IEnumerable<object>)
            {
                foreach (var company in (IEnumerable<object>)context.Object)
                {
                    FormatCsv(buffer, company);
                }
            }
            else 
            {
                FormatCsv(buffer,  context.Object);
            }

            
            await response.WriteAsync(buffer.ToString());
        }

        private static void FormatCsv(StringBuilder buffer,object obj)
        {
            if (obj is EmployeeDto)
            {
                EmployeeDto employee = (EmployeeDto)obj;
                buffer.AppendLine($"{employee.Id},\"{employee.Name},\"{employee.Age},\"{employee.Position}\"");
            }
                
            else if(obj is CompanyDto)
            {
                CompanyDto company = (CompanyDto)obj;
                buffer.AppendLine($"{company.Id},\"{company.Name},\"{company.Address }\"");
            }
            
        }
        private static void FormatCsv(StringBuilder buffer, EmployeeDto employee,object type)
        {
            if(type is EmployeeDto)
            buffer.AppendLine($"{employee.Id},\"{employee.Name},\"{employee.Age},\"{employee.Position}\"");
        }
    }
}
