#pragma warning disable CS8618
namespace UPC.Api.Model
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
		
		public string? SuccessURL { get; set; }
		
		#region Hosted Merchant Only
		public string? CardNumber { get; set; } = default!;
		public string? CardHolderName { get; set; } = default!;
		public string? CardIssueDate { get; set; } = default!;
		public string? CardExpireDate { get; set; } = default!;
		public string? CardVerificationValue { get; set; } = default!;
		#endregion
    }
}