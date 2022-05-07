using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Demo.Business;
using Demo.Model;
using Demo.Service;
using Microsoft.Extensions.Configuration;
using UPCDotNet.Model;
using System.Text.Json;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;
        private readonly IConfiguration _configuration;

        private JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
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
                extraData = JsonSerializer.Deserialize<JsonDocument>(requestData.extraData);
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
                string merchantKey = _configuration.GetValue<string>("UPC:MerchantKey");

                OrderData order = new OrderData();
                order.OrderID = Guid.NewGuid().ToString();
                order.OrderNumber = requestData.billNumber;
                order.OrderDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                order.OrderAmount = decimal.Parse(requestData.orderAmount);
                order.OrderCurrency = requestData.orderCurrency;
                order.OrderDescription = requestData.orderDescription;
                order.ExtraData = extraData;
                order.CardType = requestData.cardType;
                order.Bank = requestData.bank;
                order.Language = requestData.language;

                ServiceRequestData<OrderData> request = new();
                request.RequestID = Guid.NewGuid().ToString();
                request.RequestDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                request.RequestData = order;

                // Post Service
                string content = JsonSerializer.Serialize(request, JsonOptions);
                _logger.LogInformation("Request: " + content);

                string sha256Salt = _configuration.GetValue<string>("UPC:Salt");
                string signature = Hash(content, sha256Salt); // Hash 256
                string response = ServiceBase.Post(url, content, merchantKey, signature);

                // conver json result to object
                _logger.LogInformation("Response: " + response);
                ServiceResponseData<ResponseData> resultData =
                    JsonSerializer.Deserialize<ServiceResponseData<ResponseData>>(response, JsonOptions);

                return resultData;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new ServiceResponseData<ResponseData>()
                {
                    ResponseCode = "500",
                    ResponseMessage = "Error!!!",
                };
            }
        }

        [HttpGet]
        [Route("cancel")]
        public RedirectResult PostBackCancelGet([FromQuery] CancelData model)
        {
            string clientURL = _configuration.GetValue<string>("UPC:ClientEndPoint");
            string content = JsonSerializer.Serialize(model);
            string response = FromBase64String(model.data);

            _logger.LogInformation("Cancel URL PostBack: " + response);
            string linkRedirect = $"{clientURL}/router?method=cancel&param1={HttpUtility.UrlEncode(content)}&param2={HttpUtility.UrlEncode(response)}";
            return Redirect(linkRedirect);
        }

        [HttpPost]
        [Route("cancel")]
        public RedirectResult PostBackCancel([FromForm] CancelData model)
        {
            string clientURL = _configuration.GetValue<string>("UPC:ClientEndPoint");
            string content = JsonSerializer.Serialize(model);
            string response = FromBase64String(model.data);

            _logger.LogInformation("Cancel URL PostBack: " + response);
            string linkRedirect = $"{clientURL}/router?method=cancel&param1={HttpUtility.UrlEncode(content)}&param2={HttpUtility.UrlEncode(response)}";
            return Redirect(linkRedirect);
        }


        [HttpGet]
        [Route("result")]
        public RedirectResult PostBackResultGet([FromQuery] ResultData model)
        {
            string clientURL = _configuration.GetValue<string>("UPC:ClientEndPoint");
            string result = JsonSerializer.Serialize(model);
            string response = FromBase64String(model.data);
            _logger.LogInformation("Result URL PostBack: " + response);

            ServiceResponseData<OrderData> serviceResponse =
               JsonSerializer.Deserialize<ServiceResponseData<OrderData>>(response, JsonOptions);

            OrderData order = serviceResponse.ResponseData;
            List<string> list = new List<string>();
            list.Add($"responseCode={HttpUtility.UrlEncode(serviceResponse.ResponseCode)}");
            list.Add($"orderNumber={HttpUtility.UrlEncode(order.OrderNumber)}");
            list.Add($"orderAmount={HttpUtility.UrlEncode(order.OrderAmount.ToString())}");
            list.Add($"orderCurrency={HttpUtility.UrlEncode(order.OrderCurrency)}");
            list.Add($"orderDateTime={HttpUtility.UrlEncode(FormatDate(order.OrderDateTime))}");
            list.Add($"param1={HttpUtility.UrlEncode(result)}");
            list.Add($"param2={HttpUtility.UrlEncode(response)}");
            string url = $"{clientURL}/router?method=success&" + string.Join("&", list.ToArray());
            return Redirect(url);
        }


        [HttpPost]
        [Route("result")]
        public RedirectResult PostBackResult([FromForm] ResultData model)
        {
            string clientURL = _configuration.GetValue<string>("UPC:ClientEndPoint");
            string result = JsonSerializer.Serialize(model);
            string response = FromBase64String(model.data);
            _logger.LogInformation("Result URL PostBack: " + response);

            ServiceResponseData<OrderData> serviceResponse =
                JsonSerializer.Deserialize<ServiceResponseData<OrderData>>(response, JsonOptions);

            OrderData order = serviceResponse.ResponseData;
            List<string> list = new List<string>();
            list.Add($"responseCode={HttpUtility.UrlEncode(serviceResponse.ResponseCode)}");
            list.Add($"orderNumber={HttpUtility.UrlEncode(order.OrderNumber)}");
            list.Add($"orderAmount={HttpUtility.UrlEncode(order.OrderAmount.ToString())}");
            list.Add($"orderCurrency={HttpUtility.UrlEncode(order.OrderCurrency)}");
            list.Add($"orderDateTime={HttpUtility.UrlEncode(FormatDate(order.OrderDateTime))}");
            list.Add($"param1={HttpUtility.UrlEncode(result)}");
            list.Add($"param2={HttpUtility.UrlEncode(response)}");
            string url = $"{clientURL}/router?method=success&" + string.Join("&", list.ToArray());
            return Redirect(url);
        }

        [HttpPost]
        [Route("ipn")]
        public void PostBackIPN([FromBody] ResultData model)
        {
            string response = FromBase64String(model.data);
            _logger.LogInformation("IPN URL PostBack: " + response);
        }

        public static string FormatDate(string value)
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


        public static Dictionary<string, object> FlattenObject(PaymentData data)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            foreach (PropertyInfo property in typeof(PaymentData).GetProperties())
            {
                string key = property.Name;
                object value = property.GetValue(data);

                if (key.ToLower() == "signature")
                {
                    continue;
                }

                var aaa11 = property.PropertyType;

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string)) //key.ToLower() == "uphparameters"
                {
                    var aaa = property.PropertyType;
                    foreach (PropertyInfo item in value.GetType().GetProperties())
                    {
                        properties.Add($"{property.Name}.{item.Name}", item.GetValue(value));
                    }
                }
                else
                {
                    properties.Add(property.Name, property.GetValue(data));
                }
            }

            return properties;
        }

        public static string Hash(
            string plainText,
            string salt = "")
        {
            using (HashAlgorithm provider = new SHA256CryptoServiceProvider())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainText + salt);
                bytes = provider.ComputeHash(bytes);
                return bytes.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
            }
        }

        public static decimal FormatDecimal(string value)
        {
            if(string.IsNullOrWhiteSpace(value) == true)
            {
                return 0;
            }

            string number = new String(value.Where(Char.IsDigit).ToArray());
            return decimal.Parse(number);
        }

        public static string FormatString(string value)
        {
            if (string.IsNullOrWhiteSpace(value) == true)
            {
                return "0";
            }

            string number = new String(value.Where(Char.IsDigit).ToArray());
            return number;
        }


        public PaymentData BuildData(RequestData requestData)
        {
            //string sha256Salt = _configuration.GetValue<string>("UPC:Salt"); ///// UAT
            object extraData = JsonSerializer.Deserialize<object>(requestData.extraData);

            //ExtraData extra = new ExtraData();
            ///extra.passengers.Add(new Passenger());
            ///extra.passengers.Add(new Passenger());
            // Body
            PaymentData data = new PaymentData
            {
                billNumber = requestData.billNumber.ToString(),
                orderAmount = FormatString(requestData.orderAmount),
                orderCurrency = requestData.orderCurrency,
                orderDescription = requestData.orderDescription,
                language = requestData.language,
                cardType = requestData.cardType,
                bank = requestData.bank,
                otp = "on",
                request = requestData.request,
                token = "",
                client_ip_addr = "1.1.1.1",
                order_timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                uphParameters = new UPHParameters
                {
                    ip = "1.1.1.1",
                    agent = "Chrome 96.0",
                    device = "Windows",
                },
                extraData = extraData
            };

            // UAT
            // Create Signature 
            //Dictionary<string, object> properties = FlattenObject(data);
            //List<string> keys = properties.Keys.OrderBy(i => i, StringComparer.Ordinal).ToList();
            //StringBuilder builder = new StringBuilder();
            //foreach (string key in keys)
            //{
            //    builder.Append(properties[key]);
            //}

            //_logger.LogInformation("Flatten Object: " + builder.ToString());

            //string signature = Hash(builder.ToString(), sha256Salt);
            //data.signature = signature;
            //_logger.LogInformation("Signature: " + signature);

            //string content = JsonConvert.SerializeObject(data);
            //_logger.LogInformation("Request: " + content);

            return data;
        }

        private string FromBase64String(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
    }
}
