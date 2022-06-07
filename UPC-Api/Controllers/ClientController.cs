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

namespace UPC.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ClientController : ControllerBase
    {
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

        static ClientController()
        {
            const int interval = 1000 * 60 * 60 * 1;  // 1 hours
            _logTimer = new Timer(ProcessOnSchedule, null, interval, interval);
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
            
            try
            {
                string url = _configuration.GetValue<string>("UPC:EndPoint");
                string apiKey = _configuration.GetValue<string>("UPC:APIKey");

                OrderData order = new();
                order.OrderID = Guid.NewGuid().ToString();
                order.OrderNumber = requestData.BillNumber;
                order.OrderDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                order.OrderAmount = decimal.Parse(requestData.OrderAmount);
                order.OrderCurrency = requestData.OrderCurrency;
                order.OrderDescription = requestData.OrderDescription;
                order.ExtraData = extraData!;
                order.CardType = requestData.CardType;
                order.Bank = requestData.Bank;
                order.Language = requestData.Language;

                if (requestData.IsHostedMerchant)
                {
                    order.CardNumber = requestData.CardNumber;
                    order.CardHolderName = requestData.CardHolderName;
                    order.CardIssueDate = requestData.CardIssueDate;
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
                string sha256Salt = _configuration.GetValue<string>("UPC:Salt");
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
                    ResponseMessage = "Error!!!",
                };
            }
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

        [HttpGet]
        [Route("cancel")]
        public RedirectResult OnCancelCallback([FromQuery] CallbackData model)
        {
            return ProcessCancel(model);
        }

        [HttpPost]
        [Route("cancel")]
        public RedirectResult OnCancelPostback([FromForm] CallbackData model)
        {
            return ProcessCancel(model);
        }

        private RedirectResult ProcessCancel(CallbackData model)
        {
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
            transaction.ResponseContent = response;
            UpdateResultToCache(transaction);
            
            string url = $"{ClientUrl}/router?method=cancel&transactionID=" + order.TransactionID;
            return Redirect(url);
        }
        
        
        [HttpGet]
        [Route("result")]
        public RedirectResult OnResultCallback([FromQuery] CallbackData model)
        {
            return ProcessResult(model);
        }
        
        [HttpPost]
        [Route("result")]
        public RedirectResult OnResultPostback([FromForm] CallbackData model)
        {
            return ProcessResult(model);
        }

        private RedirectResult ProcessResult(CallbackData model)
        {
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
            transaction.ResponseContent = response;
            transaction.OrderNumber = order.OrderNumber;
            transaction.OrderAmount = order.OrderAmount.ToString(CultureInfo.InvariantCulture);
            transaction.OrderCurrency = order.OrderCurrency;
            transaction.OrderDateTime = FormatDate(order.OrderDateTime);
            UpdateResultToCache(transaction);
            
            string url = $"{ClientUrl}/router?method=success&transactionID=" + order.TransactionID;
            return Redirect(url);
        }
        
        [HttpPost]
        [Route("ipn")]
        public void OnIPNCallback([FromBody] CallbackData model)
        {
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
            transaction.IPNResponseContent = response;
            
            UpdateIpnToCache(transaction);
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
            string salt = "")
        {
            using HashAlgorithm provider = new SHA256CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(plainText + salt);
            bytes = provider.ComputeHash(bytes);
            return bytes.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
        }
        
        private string FromBase64String(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
    }
}
