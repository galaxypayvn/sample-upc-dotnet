using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UPC.Api.Model;
using UPC.Api.Service;

namespace UPC.Api.Controllers;

[ApiController]
public class PayController : ControllerBase
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
        Converters = { new NumberToStringConverter() }
    };
    
    private class NumberToStringConverter : JsonConverter<string>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(string) == typeToConvert;
        }

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                reader.TryGetDecimal(out decimal value);
                return value.ToString(CultureInfo.InvariantCulture);
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString()!;
            }

            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.Clone().ToString();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

        
    public PayController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private static string Hash(
        string plainText, 
        string? salt = "", 
        HashAlgorithm? algorithm = null,
        Encoding? encoding = null)
    {
        algorithm ??= SHA256.Create();
        encoding ??= Encoding.UTF8;

        byte[] bytes = encoding.GetBytes(plainText + salt);
        bytes = algorithm.ComputeHash(bytes);
        return bytes.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
    }
    
    [HttpPost]
    [Route("/createSignature")]
    public string CreateSignature(
        [FromHeader(Name="salt")] string sha256Salt,
        ServiceRequestData<object> request)
    {
        string content = JsonSerializer.Serialize(request, JsonOptions);
        string signature = Hash(content, sha256Salt); // Hash 256
        
        return signature!;
    }
    
    [HttpPost]
    [Route("/transaction/pay")]
    public ServiceResponseData<ResponseData> CreateOrderPay(
        [FromHeader(Name="signature")] string signature,
        [FromHeader(Name="apiKey")] string apiKey,
        ServiceRequestData<PayOrderRequest> request)
    {
        string url = _configuration.GetValue<string>("UPC:EndPoint") + "/api/v1/transaction/pay";
        string content = JsonSerializer.Serialize(request, JsonOptions);
        string response = ServiceBase.Post(url, content, apiKey, signature);

        ServiceResponseData<ResponseData>? resultData =
            JsonSerializer.Deserialize<ServiceResponseData<ResponseData>>(response, JsonOptions);

        return resultData!;
    }
    
    [HttpPost]
    [Route("/transaction/payWithOption")]
    public ServiceResponseData<ResponseData> CreateOrderPayWithOption(
        [FromHeader(Name="signature")] string signature,
        [FromHeader(Name="apiKey")] string apiKey,
        ServiceRequestData<PayWithOptionOrderRequest> request)
    {
        string url = _configuration.GetValue<string>("UPC:EndPoint") + "/api/v1/transaction/payWithOption";
        string content = JsonSerializer.Serialize(request, JsonOptions);
        string response = ServiceBase.Post(url, content, apiKey, signature);
        
        ServiceResponseData<ResponseData>? resultData =
            JsonSerializer.Deserialize<ServiceResponseData<ResponseData>>(response, JsonOptions);

        return resultData!;
    }
    
    // [HttpPost]
    // [Route("/Merchant/Refund")]
    // public ServiceResponseData<ResponseData> Refund(
    //     [FromHeader(Name="signature")] string signature,
    //     [FromHeader(Name="apiKey")] string apiKey,
    //     ServiceRequestData<RefundTransactionRequest> request)
    // {
    //     string url = _configuration.GetValue<string>("UPC:EndPoint") + "/Merchant/Refund";
    //     string content = JsonSerializer.Serialize(request, JsonOptions);
    //     string response = ServiceBase.Post(url, content, apiKey, signature);
    //     
    //     ServiceResponseData<ResponseData>? resultData =
    //         JsonSerializer.Deserialize<ServiceResponseData<ResponseData>>(response, JsonOptions);
    //
    //     return resultData!;
    // }
    
    [HttpPost]
    [Route("/transaction/query")]
    public ServiceResponseData<QueryResponse> Query(
        [FromHeader(Name="signature")] string signature,
        [FromHeader(Name="apiKey")] string apiKey,
        ServiceRequestData<QueryTransactionRequest> request)
    {
        string url = _configuration.GetValue<string>("UPC:EndPoint") + "/api/v1/transaction/query";
        string content = JsonSerializer.Serialize(request, JsonOptions);
        string response = ServiceBase.Post(url, content, apiKey, signature);
        
        ServiceResponseData<QueryResponse>? resultData =
            JsonSerializer.Deserialize<ServiceResponseData<QueryResponse>>(response, JsonOptions);

        return resultData!;
    }
    [HttpPost]
    [Route("/token/remove")]
    public ServiceResponseData<object> RemoveToken(
        [FromHeader(Name="signature")] string signature,
        [FromHeader(Name="apiKey")] string apiKey,
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