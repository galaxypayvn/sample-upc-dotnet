#pragma warning disable CS8618
namespace UPC.Api.Model;

public class RequestData
{
    public string BillNumber { get; set; }
    public string? FirstName { get; set; }
    public string? Lastname { get; set; }
    public string Language { get; set; }
    public string? City { get; set; }
    public string? Email { get; set; }

    public string OrderAmount { get; set; }
    public string OrderCurrency { get; set; }
    public string? Address { get; set; }
    public string OrderDescription { get; set; }
    public string CardType { get; set; }
    public string Bank { get; set; }
    public string ExtraData { get; set; }

    public string IntegrationMethod { get; set; }
    public string? CardNumber { get; set; } = default!;
    public string? CardHolderName { get; set; } = default!;
    public string? CardIssueDate { get; set; } = default!;
    public string? CardExpireDate { get; set; } = default!;
    public string? CardVerificationValue { get; set; } = default!;
    
    public string? ApiKey { get; set; }
    public string? Salt { get; set; }
    public string? SuccessURL { get; set; } = default!;
}