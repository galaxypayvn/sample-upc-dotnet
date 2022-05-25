namespace UPC.Api.Model
{
    public class ServiceResponseData<TModel> where TModel : class
    {
        public string RequestID { get; set; } = default!;
        public string ResponseDateTime { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmss");
        public TModel ResponseData { get; set; } = default!;
        public string ResponseCode { get; set; } = default!;
        public string ResponseMessage { get; set; } = default!;
    }
}

