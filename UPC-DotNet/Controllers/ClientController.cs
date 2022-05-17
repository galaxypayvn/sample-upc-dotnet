using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Demo.Model;
using Demo.Service;
using Microsoft.Extensions.Configuration;
using UPCDotNet.Model;
using System.Text.Json;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;
        private readonly IConfiguration _configuration;

        private JsonSerializerOptions JsonOptions { get; } = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public ClientController(ILogger<ClientController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("client")]
        public ServiceResponseData<ResponseData> Submit(RequestData requestData)
        {
            // Validate ExtraData
            JsonDocument extraData;
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
                order.ExtraData = extraData;
                order.CardType = requestData.CardType;
                order.Bank = requestData.Bank;
                order.Language = requestData.Language;

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
                ServiceResponseData<ResponseData> resultData =
                    JsonSerializer.Deserialize<ServiceResponseData<ResponseData>>(response, JsonOptions);

                return resultData;
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

            string content = JsonSerializer.Serialize(model);
            List<string> parameters = new();
            parameters.Add($"param1={HttpUtility.UrlEncode(content)}");
            parameters.Add($"param2={HttpUtility.UrlEncode(response)}");
            
            string url = $"{ClientUrl}/router?method=cancel&" + string.Join("&", parameters.ToArray());
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

            ServiceResponseData<OrderData> serviceResponse =
                JsonSerializer.Deserialize<ServiceResponseData<OrderData>>(response, JsonOptions);
            OrderData order = serviceResponse?.ResponseData ?? new();
            
            string content = JsonSerializer.Serialize(model);
            List<string> parameters = new();
            parameters.Add($"responseCode={HttpUtility.UrlEncode(serviceResponse?.ResponseCode)}");
            parameters.Add($"orderNumber={HttpUtility.UrlEncode(order.OrderNumber)}");
            parameters.Add($"orderAmount={HttpUtility.UrlEncode(order.OrderAmount.ToString(CultureInfo.InvariantCulture))}");
            parameters.Add($"orderCurrency={HttpUtility.UrlEncode(order.OrderCurrency)}");
            parameters.Add($"orderDateTime={HttpUtility.UrlEncode(FormatDate(order.OrderDateTime))}");
            parameters.Add($"param1={HttpUtility.UrlEncode(content)}");
            parameters.Add($"param2={HttpUtility.UrlEncode(response)}");
            string url = $"{ClientUrl}/router?method=success&" + string.Join("&", parameters.ToArray());
            return Redirect(url);
        }
        
        [HttpPost]
        [Route("ipn")]
        public void OnIPNCallback([FromBody] CallbackData model)
        {
            string response = FromBase64String(model.Data);
            _logger.LogInformation("IPN URL PostBack: " + response);
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
