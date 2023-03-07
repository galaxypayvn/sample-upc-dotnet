using System.ComponentModel;

namespace UPC.Api.Model;

public class ServiceRequestData<TModel> where TModel : class
{
    [DefaultValue("52dad20f7ae31b2a899e95f9558bd27c")]
    [SwaggerValue(typeof(GuidSwagger))]
    public string RequestID { get; set; } = default!;

    [DefaultValue("yyyyMMddHHmmss")]
    [SwaggerValue(typeof(DateTimeStringSwagger))]
    public string RequestDateTime { get; set; } = default!;
    
    [SwaggerValue("{}")]
    public TModel RequestData { get; set; } = default!;
}