using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UPC.Api.Model;
using UPC.Api.Service;

namespace UPC.Api.Controllers;

[ApiController]
public class TokenController : ControllerBase
{
    private readonly IConfiguration _configuration;

    private JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new PayController.NumberToStringConverter() }
    };
    
    public TokenController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    [HttpPost]
    [Route("/token/remove")]
    public ServiceResponseData<object> RemoveToken(
        [FromHeader(Name="salt")] string salt,
        [FromHeader(Name="apiKey")] string apiKey,
        [FromHeader(Name="signature")] string signature,
        ServiceRequestData<RemoveTokenRequest> request)
    {
        string url = _configuration.GetValue<string>("UPC:EndPoint") + "/api/v1/token/remove";
        string content = JsonSerializer.Serialize(request, JsonOptions);
        string response = ServiceBase.Post(url, content, apiKey, signature);
        
        ServiceResponseData<object>? resultData =
            JsonSerializer.Deserialize<ServiceResponseData<object>>(response, JsonOptions);

        return resultData!;
    }
}