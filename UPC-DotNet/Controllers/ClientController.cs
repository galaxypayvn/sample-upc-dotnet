using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

namespace Demo.Controllers
{
    [ApiController]
    [Route("api")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;
        private readonly IConfiguration _configuration;

        public ClientController(ILogger<ClientController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("client")]
        public ResponseData Submit(RequestData requestData)
        {
            try
            {
                string url = string.Empty;
                string merchantKey = _configuration.GetValue<string>("UPC:MerchantKey");

                switch (requestData.cardType)
                {
                    case "momo":
                        url = _configuration.GetValue<string>("UPC:MoMoEndPoint");    // IN DEVELOP
                        break;
                    default:
                        url = _configuration.GetValue<string>("UPC:EndPoint");
                        break;
                }

                // Post Service
                PaymentData dataBind = BuildData(requestData);
                string content = JsonConvert.SerializeObject(dataBind);
                string response = ServiceBase.Post(url, content, merchantKey);

                // conver json result to object
                _logger.LogInformation("Response: " + response);
                ResponseData resultData = System.Text.Json.JsonSerializer.Deserialize<ResponseData>(response);

                return resultData;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new ResponseData();
            }
        }

        [HttpPost]
        [Route("cancel")]
        public RedirectResult PostBackCancel([FromForm] CancelData model)
        {
            string aesKey = _configuration.GetValue<string>("UPC:AESKey");
            string clientURL = _configuration.GetValue<string>("UPC:ClientEndPoint");

            string content = JsonConvert.SerializeObject(model);
            string response = DecryptBusiness.Decrypt(aesKey, model.data);

            _logger.LogInformation("Cancel URL Postback: " + response);
            string linkRedirect = $"{clientURL}/cancel?param1={content}&param2={response}";
            return Redirect(linkRedirect);
        }

        [HttpPost]
        [Route("result")]
        public RedirectResult PostBackResult([FromForm] ResultData model)
        {
            string aesKey = _configuration.GetValue<string>("UPC:AESKey");
            string clientURL = _configuration.GetValue<string>("UPC:ClientEndPoint");

            string response = DecryptBusiness.Decrypt(aesKey, model.data);
            ResultResponseData resultData = JsonConvert.DeserializeObject<ResultResponseData>(response);
            DateTime localDatetime = DateTimeOffset.FromUnixTimeSeconds(resultData.order_timestamp).LocalDateTime;

            List<string> list = new List<string>();
            foreach (PropertyInfo property in resultData.GetType().GetProperties())
            {
                string name = property.Name;
                object value = property.GetValue(resultData);

                if (value == null)
                {
                    continue;
                }

                if (name == "order_timestamp")
                {
                    value = (localDatetime + "").ToString();

                }

                string data = (value + "").ToString();
                string content = $"{name}=" + HttpUtility.UrlEncode(data);
                list.Add(content);
            }

            _logger.LogInformation("Result URL Postback: " + response);
            string url = $"{clientURL}/success?" + string.Join("&", list.ToArray());
            return Redirect(url);
        }

        [HttpPost]
        [Route("ipn")]
        public void PostBackIPN([FromBody] ResultData model)
        {
            string aesKey = _configuration.GetValue<string>("UPC:AESKey");
            string response = DecryptBusiness.Decrypt(aesKey, model.data);
            _logger.LogInformation("IPN URL Postback: " + response);
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

                if (key.ToLower() == "uphparameters")
                {
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


        public PaymentData BuildData(RequestData requestData)
        {
            string sha256Salt = _configuration.GetValue<string>("UPC:Salt");

            // Body
            PaymentData data = new PaymentData
            {
                billNumber = requestData.billNumber.ToString(),             
                orderAmount = FormatDecimal(requestData.orderAmount),  
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
                    device = "Windows"
                },
                signature = ""
            };

            // Create Signature
            Dictionary<string, object> properties = FlattenObject(data);
            List<string> keys = properties.Keys.OrderBy(i => i, StringComparer.Ordinal).ToList();
            StringBuilder builder = new StringBuilder();
            foreach (string key in keys)
            {
                builder.Append(properties[key]);
            }

            _logger.LogInformation("Flatten Object: " + builder.ToString());

            string signature = Hash(builder.ToString(), sha256Salt);
            data.signature = signature;
            _logger.LogInformation("Signature: " + signature);

            string content = JsonConvert.SerializeObject(data);
            _logger.LogInformation("Request: " + content);

            return data;
        }
    }
}
