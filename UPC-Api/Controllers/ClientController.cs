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
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ClientController : ControllerBase
    {
        private const string ChecksumText = "{\"signature\":\"failure\"}";
        private readonly ILogger<ClientController> _logger;
        private readonly IConfiguration _configuration;
        
        // ReSharper disable once NotAccessedField.Local
        private static Timer _logTimer;

        private JsonSerializerOptions JsonOptions { get; } = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly ConcurrentDictionary<string, TransactionData> Transactions = new();
        private static readonly ConcurrentDictionary<string, MasterMerchantData.MerchantData> Merchants = new();
        private static readonly bool IsMasterMerchant;
        
        static ClientController()
        {
            const int interval = 1000 * 60 * 60 * 1;  // 1 hours
            _logTimer = new Timer(ProcessOnSchedule, null, interval, interval);
            
            // Master Merchant Only
            string folderPath = AppContext.BaseDirectory + "/Assets/config.json";
            StreamReader reader = new StreamReader(folderPath);
            string streamData = reader.ReadToEnd();
            MasterMerchantData? master = JsonSerializer.Deserialize<MasterMerchantData>(streamData);
            if (master != null &&
                master.Merchants.Count > 0)
            {
                IsMasterMerchant = true;
                foreach (MasterMerchantData.MerchantData merchant in master.Merchants)
                {
                    Merchants.TryAdd(merchant.MerchantID, merchant);
                }
            }
        }
        
        /// <summary>
        ///     Remove expired transactions from Cache
        /// </summary>
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
        public List<MasterMerchantData.MerchantData> GetMerchant()
        {
            return Merchants.Values.ToList();
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
                MasterMerchantData.MerchantData merchant;
                if (IsMasterMerchant)
                {
                    Merchants.TryGetValue(requestData.MerchantID, out merchant!);
                }
                else
                {
                    merchant = new MasterMerchantData.MerchantData();
                    merchant.MerchantID = "Demo";
                    merchant.ApiKey = _configuration.GetValue<string>($"UPC:APIKey");
                    merchant.Salt = _configuration.GetValue<string>($"UPC:Salt");
                }
                
                bool isHostedMerchant = requestData.IntegrationMethod == "HOSTED";
                bool isPayWithOption = requestData.IntegrationMethod == "OPTION";
                string route = isPayWithOption ? "payWithOption" : "pay";
                string url = _configuration.GetValue<string>("UPC:EndPoint") + "/" + route;

                OrderData order = new();
                order.ApiOperation = requestData.ApiOperation;
                order.OrderID = Guid.NewGuid().ToString();
                order.OrderNumber = requestData.BillNumber;
                order.OrderDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                order.OrderAmount = orderAmount;
                order.OrderCurrency = requestData.OrderCurrency;
                order.OrderDescription = requestData.OrderDescription;
                order.ExtraData = extraData!;
                order.Language = requestData.Language;

                order.SuccessURL = BuildURL(requestData.SuccessURL, requestData.MerchantID);
                order.FailureURL = BuildURL(requestData.SuccessURL, requestData.MerchantID);
                order.CancelURL =  BuildURL(requestData.CancelURL, requestData.MerchantID);
                order.IpnURL =  BuildURL(requestData.IpnURL, requestData.MerchantID);
                
                // Simple Checkout & Hosted Checkout
                if (isPayWithOption == false)
                {
                    order.SourceType = requestData.SourceType;
                    order.PaymentMethod = requestData.PaymentMethod;
                }
                
                // Hosted Checkout
                if (isHostedMerchant &&
                    requestData.SourceOfFund == "CARD" &&
                    requestData.PaymentMethod.ToUpper() != "WALLET" &&
                    requestData.PaymentMethod.ToUpper() != "HUB")
                {
                    order.CardNumber = requestData.CardNumber;
                    order.CardHolderName = requestData.CardHolderName;
                    order.CardExpireDate = requestData.CardExpireDate;
                    order.CardVerificationValue = requestData.CardVerificationValue;
                }
                
                // Pay with token
                if (isHostedMerchant &&
                    requestData.SourceOfFund == "TOKEN" &&
                    string.IsNullOrEmpty(requestData.Token) == false)
                {
                    order.SourceOfFund = requestData.SourceOfFund;
                    order.Token = requestData.Token;
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
                string response = ServiceBase.Post(url, content, merchant.ApiKey, signature);

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
            bool isMatch = VerifySignature(model, merchant);
            _logger.LogInformation("Cancel verify signature data: " + isMatch);

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
            transaction.ResponseContent = isMatch ? response : ChecksumText;
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
            bool isMatch = VerifySignature(model, merchant);
            _logger.LogInformation("Result verify signature data: " + isMatch);

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
            transaction.ResponseContent = isMatch ? response : ChecksumText;
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
            bool isMatch = VerifySignature(model, merchant);
            _logger.LogInformation("IPN verify signature data: " + isMatch);

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
            transaction.IPNResponseContent = isMatch ? response : ChecksumText;
            
            UpdateIpnToCache(transaction);
        }

        private bool VerifySignature(CallbackData? model, string merchantId)
        {
            if (model == null)
            {
                return false;
            }

            string salt;
            if (IsMasterMerchant)
            {
                Merchants.TryGetValue(merchantId, out MasterMerchantData.MerchantData? merchant);
                salt = merchant?.Salt + string.Empty;
            }
            else
            {
                salt = _configuration.GetValue<string>("UPC:Salt");
            }
            
            string signature = Hash(model.Data, salt);
            return signature == model.Signature;
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
                
                return $"http://{host}";
            }
        }

        private string? BuildURL(string? url, string? merchant)
        {
            if (url == null)
            {
                return null;
            }
            
            if (string.IsNullOrWhiteSpace(merchant))
            {
                merchant = "Demo";
            }

            return $"{url}/{merchant}";
        }
    }
}
