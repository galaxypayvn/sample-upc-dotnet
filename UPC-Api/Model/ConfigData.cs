using System.Text.Json.Serialization;
#pragma warning disable CS8618

namespace UPC.Api.Model;

public class ConfigData
{
    public List<Merchant> Merchants { get; set; }
}

public class Merchant
{
    [JsonPropertyName("value")]
    public string MerchantID { get; set; }
    
    [JsonPropertyName("text")]
    public string MerchantName { get; set; }
    
    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; }
    
    [JsonPropertyName("salt")]
    public string Salt { get; set; }
}