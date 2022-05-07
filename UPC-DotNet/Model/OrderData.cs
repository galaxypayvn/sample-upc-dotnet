
namespace UPCDotNet.Model
{
	public class OrderData
	{
		public string OrderID { get; set; }
		public string OrderNumber { get; set; }
		public decimal OrderAmount { get; set; }
		public string OrderCurrency { get; set; }
		public string OrderDateTime { get; set; }
		public string OrderDescription { get; set; }

		public string CardType { get; set; }
		public string Bank { get; set; }
		public string OTP { get; set; } = "on";
		public string Request { get; set; } = "purchase";
		public object ExtraData { get; set; }
		public object Language { get; set; }
    }
}