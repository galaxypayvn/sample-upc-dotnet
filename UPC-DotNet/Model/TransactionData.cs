namespace UPCDotNet.Model;

public class TransactionData
{
    public string TransactionID { get; set; }
    
    public string RawContent { get; set; }
    public string ResponseContent { get; set; }
    
    public string ResponseCode { get; set; }
    public string OrderNumber { get; set; }
    public string OrderAmount { get; set;}
    public string OrderCurrency { get; set; }
    public string OrderDateTime { get; set; }
}