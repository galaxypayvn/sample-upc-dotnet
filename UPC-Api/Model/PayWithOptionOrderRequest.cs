using System.ComponentModel;

namespace UPC.Api.Model;

public class PayWithOptionOrderRequest
{
    [DefaultValue("PAY or PAY_WITH_CREATE_TOKEN")]
        [SwaggerValue("PAY")]
        public string ApiOperation { get; set; }
        
        [DefaultValue("Guid")]
        [SwaggerValue(typeof(GuidSwagger))]
        public string OrderID { get; set; }
        
        [DefaultValue("Random")]
        [SwaggerValue(typeof(GuidSwagger))]
        public string OrderNumber { get; set; }
        
        [DefaultValue(100000)]
        [SwaggerValue(100000)]
        public decimal OrderAmount { get; set; }
        
        [DefaultValue("VND, USD, ...")]
        [SwaggerValue("VND")]
        public string OrderCurrency { get; set; }
        
        [DefaultValue("yyyyMMddHHmmss")]
        [SwaggerValue(typeof(DateTimeStringSwagger))]
        public string OrderDateTime { get; set; }
        
        [DefaultValue("Description")]
        [SwaggerValue("pay demo transaction")]
        public string OrderDescription { get; set; }

        [DefaultValue("{}")]
        [SwaggerValue("{}")]
        public object ExtraData { get; set; }
        
        [DefaultValue("vi, en")]
        [SwaggerValue("vi")]
        public string Language { get; set; }

        #region Pay with token
        [DefaultValue("3235FADF2D3814114D509D2D1C2E1CAB5652")]
        [SwaggerValue(" ")]
        public string? Token { get; set; }
        
        [DefaultValue("TOKEN, CARD")]
        [SwaggerValue(" ")]
        public string? SourceOfFund { get; set; }
        #endregion
        
        #region url callback
        [DefaultValue("https://uat-merchant.galaxypay.vn/api/result")]
        [SwaggerValue(" ")]
        public string? SuccessURL { get; set; }
        
        [DefaultValue("https://uat-merchant.galaxypay.vn/api/result")]
        [SwaggerValue(" ")]
        public string? FailureURL { get; set; }
        
        [DefaultValue("https://uat-merchant.galaxypay.vn/api/cancel")]
        [SwaggerValue(" ")]
        public string? CancelURL { get; set; }
        
        [DefaultValue("https://uat-merchant.galaxypay.vn/api/ipn")]
        [SwaggerValue(" ")]
        public string? IpnURL { get; set; }
        #endregion
}