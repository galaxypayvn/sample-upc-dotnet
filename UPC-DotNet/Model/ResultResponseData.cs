namespace Demo.Model
{
    public class ResultResponseData
    {
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public long pay_timestamp { get; set; }
        public decimal pay_amount { get; set; }
        public string language { get; set; }
        public string orderId { get; set; }
        public string billNumber { get; set; }
        public decimal orderAmount { get; set; }
        public string orderDescription { get; set; }
        public string orderCurrency { get; set; }
        public string cardType { get; set; }
        public string cardScheme { get; set; }
        public string token { get; set; }
        public long order_timestamp { get; set; }
        public string client_ip_addr { get; set; }
    }
}
