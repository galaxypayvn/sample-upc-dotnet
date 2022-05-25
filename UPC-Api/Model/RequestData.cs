namespace UPC.Api.Model
{
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
    }
}
