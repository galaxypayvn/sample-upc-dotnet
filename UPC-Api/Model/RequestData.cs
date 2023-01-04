#pragma warning disable CS8618

namespace UPC.Api.Model;

public class RequestData
{
    public string ApiOperation { get; set; }
    public string BillNumber { get; set; }
    public string Language { get; set; }
    public string OrderAmount { get; set; }
    public string OrderCurrency { get; set; }
    public string OrderDescription { get; set; }
    public string PaymentMethod { get; set; }
    public string SourceType { get; set; }
    public string ExtraData { get; set; }

    public string IntegrationMethod { get; set; }
    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? CardExpireDate { get; set; }
    public string? CardVerificationValue { get; set; }

    public string MerchantID { get; set; }
    public string? SuccessURL { get; set; }
    public string? CancelURL { get; set; }
    public string? IpnURL { get; set; }
    
    public string BaseUrl { get; set; }
    public string? Token { get; set; }
    public string? SourceOfFund { get; set; }
}