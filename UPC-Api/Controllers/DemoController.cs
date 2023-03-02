using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UPC.Api.Model;
using UPC.Api.Service;

namespace UPC.Api.Controllers;

[ApiController]
public class DemoController : ControllerBase
{
    private readonly ILogger<ClientController> _logger;
    private readonly IConfiguration _configuration;
        
    // ReSharper disable once NotAccessedField.Local
    private static Timer _logTimer;

    private JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    [HttpPost]
    [Route("/pay")]
    public ServiceResponseData<ResponseData> CreateOrderPay(
        [FromHeader(Name="signature")] string signature,
        [FromHeader(Name="apiKey")] string apiKey,
        ServiceRequestData<OrderData> request)
    {
        string url = _configuration.GetValue<string>("UPC:EndPoint");
        
        string content = JsonSerializer.Serialize(request, JsonOptions);
        string response = ServiceBase.Post(url, content, apiKey, signature);
        
        ServiceResponseData<ResponseData>? resultData =
            JsonSerializer.Deserialize<ServiceResponseData<ResponseData>>(response, JsonOptions);

        return resultData!;
    }
    
    [HttpPost]
    [Route("/payWithOption")]
    public ServiceResponseData<ResponseData> CreateOrderPayWithOption(
        [FromHeader(Name="signature")] string signature,
        [FromHeader(Name="apiKey")] string apiKey,
        ServiceRequestData<OrderData> request)
    {
        string url = _configuration.GetValue<string>("UPC:EndPoint");
        
        string content = JsonSerializer.Serialize(request, JsonOptions);
        string response = ServiceBase.Post(url, content, apiKey, signature);
        
        ServiceResponseData<ResponseData>? resultData =
            JsonSerializer.Deserialize<ServiceResponseData<ResponseData>>(response, JsonOptions);

        return resultData!;
    }
}