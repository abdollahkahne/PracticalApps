using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Formatters
{
    public class CSVOutputFormatter : TextOutputFormatter
    {
        // In Constructor we can specify supported Media Types and Encodings
        // Media Types can be determined by Media Type Header Value or string
        public CSVOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }
        // With this method we can enable result Object types that can be format using this formatter
        // We can also specify directly using result itself instead/in addition to using resut Type
        protected override bool CanWriteType(Type type)
        {
            if (typeof(CompanyDto).IsAssignableFrom(type) || typeof(IEnumerable<CompanyDto>).IsAssignableFrom(type))
            {
                return base.CanWriteType(type); // This should return instead of true
            }
            else
            {
                return false;
            }
        }

        // using this method we can write to body of resonse as our formatter should do
        // we can use WriteResponseHeader and WriteResponseAsync if we need more options
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            // Get Response Object from http context to write to
            var response = context.HttpContext.Response;

            // Create a String Builder
            var buffer = new StringBuilder();
            addHeadColumns(buffer);

            // Context have information about result including content type, its object and its type
            // It also have HttpContext Object
            if (context.Object is IEnumerable<CompanyDto>)
            {
                foreach (var item in context.Object as IEnumerable<CompanyDto>)
                {
                    formatCsv(buffer, item);
                }
            }
            else
            {
                formatCsv(buffer, (CompanyDto)context.Object);
            }
            await response.WriteAsync(buffer.ToString()); // How to apply selected encoding here??
        }

        private static void formatCsv(StringBuilder buffer, CompanyDto company)
        {
            buffer.AppendLine($"{company.Id},\"{company.Name}\",\"{company.FullAddress}\"");
        }
        private static void addHeadColumns(StringBuilder buffer)
        {
            buffer.AppendLine("Id,Name,\"Full Address\"");
        }
    }
}