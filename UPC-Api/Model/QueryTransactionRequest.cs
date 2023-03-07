using System.ComponentModel;

namespace UPC.Api.Model;
#pragma warning disable CS8618

public class QueryTransactionRequest
{
    [DefaultValue("2303075213072804600236536")]
    [SwaggerValue("2303075213072804600236536")]
    public string TransactionID { get; set; }
}