using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UPC.Api.Model;

namespace UPC.Api.Controllers;

[ApiController]
public class AuthController : ControllerBase
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
    
    public AuthController(IConfiguration configuration)
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
}