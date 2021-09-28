using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace NorthwindIntl.ValueProviders
{
    public class XMLResult : ActionResult
    {
        public XMLResult(object value)
        {
            this.Value = value;

        }
        public object Value { get; }
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (Value!=null) {
                var serializer=new XmlSerializer(Value.GetType());
                using (var ms=new MemoryStream())
                {
                     serializer.Serialize(ms,Value);
                     var data=ms.ToArray();
                     context.HttpContext.Response.ContentType="application/xml";
                     context.HttpContext.Response.ContentLength=data.Length;
                     context.HttpContext.Response.Body.WriteAsync(data,0,data.Length);
                }
            }
            return base.ExecuteResultAsync(context);
        }
    }
}