namespace UPC.Api.Model;
#pragma warning disable CS8618

public class QueryResponse
{
    public string TransactionID { get; set; }
    public string TransactionStatus { get; set; }
    public string TransactionDescription { get; set; }
    public string OrderID { get; set; }
    public string OrderNumber { get; set; }
    public string OrderAmount { get; set; }
    public string OrderDescription { get; set; }
    public string OrderCurrency { get; set; }
    public string OrderDateTime { get; set; }
}