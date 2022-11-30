#pragma warning disable CS8618
namespace UPC.Api.Model;

public class RequestData
{
    public string BillNumber { get; set; }
    public string Language { get; set; }
    public string OrderAmount { get; set; }
    public string OrderCurrency { get; set; }
    public string OrderDescription { get; set; }
    public string PaymentMethod { get; set; }
    public string SourceType { get; set; }
    public string ExtraData { get; set; }

    public string IntegrationMethod { get; set; }
    public string? CardNumber { get; set; } = default!;
    public string? CardHolderName { get; set; } = default!;
    public string? CardExpireDate { get; set; } = default!;
    public string? CardVerificationValue { get; set; } = default!;

    public string MerchantID { get; set; } = "1";
    public string? SuccessURL { get; set; } = default!;
    public string? IpnURL { get; set; } = default!;
}