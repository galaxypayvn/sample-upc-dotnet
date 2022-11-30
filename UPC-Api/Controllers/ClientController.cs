using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using UPC.Api.Model;
using UPC.Api.Service;

#pragma warning disable CS0618

namespace UPC.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ClientController : ControllerBase
    {
        private readonly string _checksumText = "{\"checksum\":\"failure\"}";
        private readonly ILogger<ClientController> _logger;
        private readonly IConfiguration _configuration;
        private static Timer _logTimer;

        private JsonSerializerOptions JsonOptions { get; } = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly ConcurrentDictionary<string, TransactionData> Transactions = new();
        private static readonly ConcurrentDictionary<string, Merchant> Merchants = new();

        static ClientController()
        {
            const int interval = 1000 * 60 * 60 * 1;  // 1 hours
            _logTimer = new Timer(ProcessOnSchedule, null, interval, interval);
            
            // Read file config
            string folderPath = AppContext.BaseDirectory + "/Assets/config.json";
            StreamReader reader = new StreamReader(folderPath);
            string streamData = reader.ReadToEnd();
            
            // Add merchants to cache
            ConfigData? configData = JsonSerializer.Deserialize<ConfigData>(streamData);
            if (configData != null)
            {
                List<Merchant> merchants = configData.Merchants;
                foreach (Merchant merchant in merchants)
                {
                    Merchants.TryAdd(merchant.MerchantID, merchant);
                }
            }
        }
        
        // Remove expired transactions from Cache
        private static void ProcessOnSchedule(object? state)
        {
            long currentDateTime = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            
            List<TransactionData> expiredTransactions =
            Transactions.Values
                .Where(item => item.DateTimeExpire < currentDateTime)
                .ToList();

            foreach (TransactionData transaction in expiredTransactions)
            {
                Transactions.TryRemove(transaction.TransactionID, out _);
            }
        }
        
        public ClientController(ILogger<ClientController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        
        [HttpGet]
        [Route("merchant")]
        public List<Merchant> GetMerchant()
        {
            List<Merchant> merchants = new List<Merchant>();
            foreach (Merchant merchant in Merchants.Values)
            {
                merchants.Add(merchant);
            }

            return merchants;
        }

        [HttpPost]
        [Route("transaction")]
        public TransactionData Get([FromBody] TransactionData transaction)
        {
            if (string.IsNullOrWhiteSpace(transaction.TransactionID))
            {
                return new TransactionData();
            }

            if (Transactions.TryGetValue(transaction.TransactionID, out transaction!) == false)
            {
                transaction = new TransactionData();
            }

            return transaction;
        }

        private static void AddCache(TransactionData transaction)
        {
            transaction.DateTimeExpire = long.Parse(DateTime.Now.AddDays(1).ToString("yyyyMMddHHmmss"));
            Transactions.TryAdd(transaction.TransactionID, transaction);
        }
        
        private static void UpdateResultToCache(TransactionData transaction)
        {
            if (Transactions.TryGetValue(transaction.TransactionID, out TransactionData? source) == false)
            {
                AddCache(transaction);
            }
            else
            {
                source.ResultResponseTime = transaction.ResultResponseTime;
                source.RawContent = transaction.RawContent;
                source.ResponseContent = transaction.ResponseContent;

                source.ResponseCode = transaction.ResponseCode;
                source.OrderNumber = transaction.OrderNumber;
                source.OrderAmount = transaction.OrderAmount;
                source.OrderCurrency = transaction.OrderCurrency;
                source.OrderDateTime = transaction.OrderDateTime;
            }
        }

        private static void UpdateIpnToCache(TransactionData transaction)
        {
            if (Transactions.TryGetValue(transaction.TransactionID, out TransactionData? source) == false)
            {
                AddCache(transaction);
            }
            else
            {
                source.IPNResponseTime = transaction.IPNResponseTime;
                source.IPNRawContent = transaction.IPNRawContent;
                source.IPNResponseContent = transaction.IPNResponseContent;
            }
        }
        
        
        [HttpPost]
        [Route("client")]
        public ServiceResponseData<ResponseData> Submit(RequestData requestData)
        {
            // Validate ExtraData
            JsonDocument? extraData;
            try
            {
                extraData = JsonSerializer.Deserialize<JsonDocument>(requestData.ExtraData);
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
                return new ServiceResponseData<ResponseData>()
                {
                    ResponseCode = "400",
                    ResponseMessage = "Extra data is not json",
                };
            }

            if (decimal.TryParse(
                    requestData.OrderAmount, 
                    NumberStyles.Any, 
                    CultureInfo.InvariantCulture,
                    out decimal orderAmount) == false)
            {
                return new ServiceResponseData<ResponseData>()
                {
                    ResponseCode = "400",
                    ResponseMessage = "Order Amount is invalid.",
                };
            }
            
            try
            {
                Merchants.TryGetValue(requestData.MerchantID, out Merchant? merchant);
                if (merchant == null)
                {
                    merchant = new Merchant();
                    merchant.Salt = _configuration.GetValue<string>($"UPC:Salt");
                    merchant.ApiKey = _configuration.GetValue<string>($"UPC:APIKey");
                }

                string failureUrl = requestData.BaseUrl + "api/result";
                string cancelUrl = requestData.BaseUrl + "api/cancel";
                string successUrl = requestData.BaseUrl + "api/result";
                string ipnUrl = requestData.BaseUrl + "api/ipn";
                
                bool isHostedMerchant = requestData.IntegrationMethod == "HOSTED";
                bool isPayWithOption = requestData.IntegrationMethod == "OPTION";
                string route = isPayWithOption ? "payWithOption" : "pay";
                string url = _configuration.GetValue<string>("UPC:EndPoint") + "/" + route;
                string apiKey = merchant.ApiKey;

                OrderData order = new();
                order.OrderID = Guid.NewGuid().ToString();
                order.OrderNumber = requestData.BillNumber;
                order.OrderDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                order.OrderAmount = orderAmount;
                order.OrderCurrency = requestData.OrderCurrency;
                order.OrderDescription = requestData.OrderDescription;
                order.ExtraData = extraData!;
                order.Language = requestData.Language;

                order.FailureURL = CallBackUrl(failureUrl, requestData.MerchantID);
                order.CancelURL = CallBackUrl(cancelUrl, requestData.MerchantID);
                order.SuccessURL = requestData.SuccessURL;
                order.IpnURL = requestData.IpnURL;
                
                if (string.IsNullOrEmpty(requestData.SuccessURL) || requestData.SuccessURL == successUrl)
                {
                    order.SuccessURL = CallBackUrl(successUrl, requestData.MerchantID);
                }
                
                if (string.IsNullOrEmpty(requestData.IpnURL) || requestData.IpnURL == ipnUrl)
                {
                    order.IpnURL = CallBackUrl(ipnUrl, requestData.MerchantID);
                }
                
                // Simple Checkout & Hosted Checkout
                if (isPayWithOption == false)
                {
                    order.SourceType = requestData.SourceType;
                    order.PaymentMethod = requestData.PaymentMethod;
                }
                
                // Hosted Checkout
                if (isHostedMerchant &&
                    requestData.PaymentMethod.ToUpper() != "WALLET" &&
                    requestData.PaymentMethod.ToUpper() != "HUB")
                {
                    order.CardNumber = requestData.CardNumber;
                    order.CardHolderName = requestData.CardHolderName;
                    order.CardExpireDate = requestData.CardExpireDate;
                    order.CardVerificationValue = requestData.CardVerificationValue;
                }

                ServiceRequestData<OrderData> request = new();
                request.RequestID = Guid.NewGuid().ToString("N");
                request.RequestDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                request.RequestData = order;

                string content = JsonSerializer.Serialize(request, JsonOptions);
                _logger.LogInformation("Request: " + content);

                // Request to API /transaction
                string sha256Salt = merchant.Salt;
                string signature = Hash(content, sha256Salt); // Hash 256
                string response = ServiceBase.Post(url, content, apiKey, signature);

                // Response
                _logger.LogInformation("Response: " + response);
                ServiceResponseData<ResponseData>? resultData =
                    JsonSerializer.Deserialize<ServiceResponseData<ResponseData>>(response, JsonOptions);

                return resultData!;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new ServiceResponseData<ResponseData>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Sorry, The Server is busy. Please try again later.",
                };
            }
        }

        [HttpGet]
        [Route("cancel/{merchant}")]
        public RedirectResult OnCancelCallback([FromQuery] CallbackData model, string merchant)
        {
            return ProcessCancel(model, merchant);
        }

        [HttpPost]
        [Route("cancel/{merchant}")]
        public RedirectResult OnCancelPostback([FromForm] CallbackData model, string merchant)
        {
            return ProcessCancel(model, merchant);
        }

        private RedirectResult ProcessCancel(CallbackData model, string merchant)
        {
            bool checkSum = CheckSumData(model, merchant);
            _logger.LogInformation("Cancel checksum data: " + checkSum);

            string response = FromBase64String(model.Data);
            _logger.LogInformation("Cancel URL Callback: " + response);

            ServiceResponseData<OrderResponseData>? serviceResponse =
                JsonSerializer.Deserialize<ServiceResponseData<OrderResponseData>>(response, JsonOptions);
            
            OrderResponseData order = serviceResponse?.ResponseData ??  new OrderResponseData();
            if (string.IsNullOrWhiteSpace(order.TransactionID))
            {
                order.TransactionID = Guid.NewGuid().ToString();
            }
            
            string content = JsonSerializer.Serialize(model);
            TransactionData transaction = new();
            transaction.TransactionID = order.TransactionID;
            transaction.RawContent = content;
            transaction.ResponseContent = checkSum ? response : _checksumText;
            UpdateResultToCache(transaction);
            
            string url = $"{ClientUrl}/router?method=cancel&transactionID=" + order.TransactionID;
            return Redirect(url);
        }
        
        
        [HttpGet]
        [Route("result/{merchant}")]
        public RedirectResult OnResultCallback([FromQuery] CallbackData model, string merchant)
        {
            return ProcessResult(model, merchant);
        }
        
        [HttpPost]
        [Route("result/{merchant}")]
        public RedirectResult OnResultPostback([FromForm] CallbackData model, string merchant)
        {
            return ProcessResult(model, merchant);
        }

        private RedirectResult ProcessResult(CallbackData model, string merchant)
        {
            bool checkSum = CheckSumData(model, merchant);
            _logger.LogInformation("Result checksum data: " + checkSum);

            string response = FromBase64String(model.Data);
            _logger.LogInformation("Result URL Callback: " + response);

            ServiceResponseData<OrderResponseData>? serviceResponse =
                JsonSerializer.Deserialize<ServiceResponseData<OrderResponseData>>(response, JsonOptions);
            
            OrderResponseData order = serviceResponse?.ResponseData ??  new OrderResponseData();
            if (string.IsNullOrWhiteSpace(order.TransactionID))
            {
                order.TransactionID = Guid.NewGuid().ToString();
            }
            
            string content = JsonSerializer.Serialize(model);
            TransactionData transaction = new();
            transaction.TransactionID = order.TransactionID;
            transaction.ResultResponseTime = GetDateString(serviceResponse?.ResponseDateTime);
            transaction.ResponseCode = serviceResponse?.ResponseCode;
            transaction.RawContent = content;
            transaction.ResponseContent = checkSum ? response : _checksumText;
            transaction.OrderNumber = order.OrderNumber;
            transaction.OrderAmount = order.OrderAmount.ToString(CultureInfo.InvariantCulture);
            transaction.OrderCurrency = order.OrderCurrency;
            transaction.OrderDateTime = FormatDate(order.OrderDateTime);
            UpdateResultToCache(transaction);
            
            string url = $"{ClientUrl}/router?method=success&transactionID=" + order.TransactionID;
            return Redirect(url);
        }
        
        [HttpPost]
        [Route("ipn/{merchant}")]
        public void OnIPNCallback([FromBody] CallbackData model, string merchant)
        {
            bool checkSum = CheckSumData(model, merchant);
            _logger.LogInformation("IPN checksum data: " + checkSum);

            string response = FromBase64String(model.Data);
            _logger.LogInformation("IPN URL PostBack: " + response);
            
            ServiceResponseData<OrderResponseData>? serviceResponse =
                JsonSerializer.Deserialize<ServiceResponseData<OrderResponseData>>(response, JsonOptions);
            
            OrderResponseData order = serviceResponse?.ResponseData ??  new OrderResponseData();
            if (string.IsNullOrWhiteSpace(order.TransactionID))
            {
                order.TransactionID = Guid.NewGuid().ToString();
            }
            
            string content = JsonSerializer.Serialize(model);
            TransactionData transaction = new();
            transaction.TransactionID = order.TransactionID;
            transaction.IPNResponseTime = GetDateString(serviceResponse?.ResponseDateTime);
            transaction.ResponseCode = serviceResponse?.ResponseCode;
            transaction.IPNRawContent = content;
            transaction.IPNResponseContent = checkSum ? response : _checksumText;
            
            UpdateIpnToCache(transaction);
        }

        private bool CheckSumData(CallbackData? model, string merchantId)
        {
            if (model == null)
            {
                return false;
            }
            
            Merchants.TryGetValue(merchantId, out Merchant? merchant);
            if (merchant == null)
            {
                merchant = new Merchant();
                merchant.Salt = _configuration.GetValue<string>($"UPC:Salt");
            }
            
            string signature = Hash(model.Data, merchant.Salt);
            
            if (signature == model.Signature)
            {
                return true;
            }

            return false;
        }

        private static string GetDateString(string? value)
        {
            DateTime responseDate = DateTime.Now;
            
            if (string.IsNullOrWhiteSpace(value) == false)
            {
                bool result = DateTime.TryParseExact(
                    value, 
                    "yyyyMMddHHmmss", 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None,
                    out responseDate);

                if (result == false)
                {
                    responseDate = DateTime.Now;
                }
            }
            
            return responseDate.ToString("dd/MM/yyyy HH:mm:ss");
        }
        
        private static string FormatDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return DateTime.TryParseExact(
                    value,
                    "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date)
                ? date.ToString("dd/MM/yyyy HH:mm:ss")
                : string.Empty;
        }

        private static string Hash(
            string plainText, 
            string? salt = "", 
            HashAlgorithm? algorithm = null,
            Encoding? encoding = null)
        {
            algorithm ??= SHA256.Create();
            encoding ??= Encoding.UTF8;

            byte[] bytes = encoding.GetBytes(plainText + salt);
            bytes = algorithm.ComputeHash(bytes);
            return bytes.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
        }
        
        private string FromBase64String(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        
        private string ClientUrl
        {
            get
            {
                HttpRequest request = HttpContext.Request;
                string host = request.Host.ToString();
                string protocol = request.IsHttps ? "https" : "http";

                return $"{protocol}://{host}";
            }
        }

        private string? CallBackUrl(string? url, string? merchant)
        {
            if (string.IsNullOrWhiteSpace(merchant))
            {
                merchant = "null";
            }
            
            return url + "/" + merchant;
        }
    }
}
