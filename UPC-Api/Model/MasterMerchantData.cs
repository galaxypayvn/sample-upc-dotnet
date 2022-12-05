#pragma warning disable CS8618

using System.Text.Json.Serialization;

namespace UPC.Api.Model;

public class MasterMerchantData
{
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<MerchantData> Merchants { get; set; }
    
    public class MerchantData
    {
        [JsonPropertyName("value")]
        public string MerchantID { get; set; }
    
        [JsonPropertyName("text")]
        public string MerchantName { get; set; }
    
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }
    
        [JsonPropertyName("salt")]
        public string Salt { get; set; }
    
        [JsonPropertyName("order")]
        public int Order { get; set; }
    }
}

