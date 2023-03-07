using System.ComponentModel;

namespace UPC.Api.Model;
#pragma warning disable CS8618

public class RefundTransactionRequest
{
    [DefaultValue("2303075354145183856823677")]
    [SwaggerValue("2303075354145183856823677")]
    public string TransactionID { get; set; }
    
    [DefaultValue("Guid")]
    [SwaggerValue(typeof(GuidSwagger))]
    public string OrderID { get; set; }
    
    [DefaultValue(100000)]
    [SwaggerValue(100000)]
    public string OrderAmount { get; set; }
    
    [DefaultValue("VND, USD, ...")]
    [SwaggerValue("VND")]
    public string OrderCurrency { get; set; }
    
    [DefaultValue("Description")]
    [SwaggerValue("DEMO REFUND TRANSACTION")]
    public string OrderDescription { get; set; }
}