namespace UPCDotNet.Model
{
    public class ServiceRequestData<TModel> where TModel : class
    {
        public string RequestID { get; set; } = default!;

        public string RequestDateTime { get; set; } = default!;

        public TModel RequestData { get; set; } = default!;
    }
}