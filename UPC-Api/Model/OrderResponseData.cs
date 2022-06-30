#pragma warning disable CS8618
namespace UPC.Api.Model
{
	public class OrderResponseData
	{
		public string TransactionID { get; set; }
		public string OrderID { get; set; }
		public string OrderNumber { get; set; }
		public decimal OrderAmount { get; set; }
		public string OrderCurrency { get; set; }
		public string OrderDateTime { get; set; }
		public string OrderDescription { get; set; }

		public string CardType { get; set; }
		public string Bank { get; set; }
		public object ExtraData { get; set; }
		public object Language { get; set; }
    }
}