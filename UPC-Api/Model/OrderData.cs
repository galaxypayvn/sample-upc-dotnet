#pragma warning disable CS8618
namespace UPC.Api.Model
{
	public class OrderData
	{
		public string ApiOperation { get; set; } = "PAY";
		public string OrderID { get; set; }
		public string OrderNumber { get; set; }
		public decimal OrderAmount { get; set; }
		public string OrderCurrency { get; set; }
		public string OrderDateTime { get; set; }
		public string OrderDescription { get; set; }

		public string PaymentMethod { get; set; }
		public string SourceType { get; set; }
		public object ExtraData { get; set; }
		public object Language { get; set; }
		
		public string? SuccessURL { get; set; }
		public string? FailureURL { get; set; }
		public string? CancelURL { get; set; }
		public string? IpnURL { get; set; }
		
		#region Hosted Merchant Only
		public string? CardNumber { get; set; } = default!;
		public string? CardHolderName { get; set; } = default!;
		public string? CardExpireDate { get; set; } = default!;
		public string? CardVerificationValue { get; set; } = default!;
		#endregion
    }
}