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

                // DEV
                ////string content = JsonConvert.SerializeObject(data);
                //_logger.LogInformation("content: " + content);
                //string sha256Salt = _configuration.GetValue<string>("UPC:Salt");
                //string signature = Hash(content, sha256Salt);
                //string response = ServiceBase.Post(url, content, merchantKey, signature);

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

            _logger.LogInformation("Cancel URL PostBack: " + response);
            string linkRedirect = $"{clientURL}/router?method=cancel&param1={HttpUtility.UrlEncode(content)}&param2={HttpUtility.UrlEncode(response)}";
            return Redirect(linkRedirect);
        }

        [HttpPost]
        [Route("result")]
        public RedirectResult PostBackResult([FromForm] ResultData model)
        {
            string aesKey = _configuration.GetValue<string>("UPC:AESKey");
            string clientURL = _configuration.GetValue<string>("UPC:ClientEndPoint");

            string result = JsonConvert.SerializeObject(model);
            string response = DecryptBusiness.Decrypt(aesKey, model.data);
            _logger.LogInformation("Result URL PostBack: " + response);

            ResultResponseData resultData = JsonConvert.DeserializeObject<ResultResponseData>(response);
            DateTime payDate = DateTimeOffset.FromUnixTimeSeconds(resultData.pay_timestamp).LocalDateTime;

            List<string> list = new List<string>();
            foreach (PropertyInfo property in resultData.GetType().GetProperties())
            {
                string name = property.Name;
                object value = property.GetValue(resultData);

                if (value == null)
                {
                    continue;
                }

                if (name == nameof(resultData.pay_timestamp))
                {
                    value = payDate.ToString("dd/MM/yyyy HH:mm:ss");

                }

                string data = (value + "").ToString();
                string content = $"{name}=" + HttpUtility.UrlEncode(data);
                list.Add(content);
            }

            list.Add($"param1={HttpUtility.UrlEncode(result)}");
            list.Add($"param2={HttpUtility.UrlEncode(response)}");

            string url = $"{clientURL}/router?method=success&" + string.Join("&", list.ToArray());
            return Redirect(url);
        }

        [HttpPost]
        [Route("ipn")]
        public void PostBackIPN([FromBody] ResultData model)
        {
            string aesKey = _configuration.GetValue<string>("UPC:AESKey");
            string response = DecryptBusiness.Decrypt(aesKey, model.data);
            _logger.LogInformation("IPN URL PostBack: " + response);
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
            string sha256Salt = _configuration.GetValue<string>("UPC:Salt"); ///// UAT
            string extraJson =
              "{\"customer\":{\"first_name\":\"Ryleigh\",\"last_name\":\"Rowan\",\"identity_number\":\"9184091267\",\"email\":\"Luna@gmail.com\",\"phone_number\":\"0973860571\",\"phone_type\":\"lqQPi0GSkY\",\"gender\":\"F\",\"date_of_birth\":\"20050507\",\"title\":\"Mrs\"},\"device\":{\"first_name\":null,\"last_name\":null,\"identity_number\":null,\"email\":null,\"phone_number\":null,\"phone_type\":null,\"gender\":null,\"date_of_birth\":null,\"title\":null},\"airline\":{\"record_locator\":\"oYnTcyPaJ4\",\"flights\":[{\"airline_code\":\"FvZ5fsrDKm\",\"carrier_code\":\"fY1QUIT8Gs\",\"journey_type\":1931885996,\"flight_number\":950679163,\"travel_class\":\"ifxCnjyyB3\",\"exchange_ticket_number\":\"sDxvhHiFMJ\",\"departure_airport\":\"mc4KDBOKxP\",\"departure_date\":\"YQ8YbQ0fEe\",\"departure_time\":\"21/04/202213:31:05\",\"departure_tax\":\"tJeyh9nDC2\",\"arrival_airport\":\"Tq5ZU9d1bD\",\"arrival_date\":\"ueA944TPZE\",\"arrival_time\":\"21/04/202203:30:21\",\"fees\":712003610,\"taxes\":749099820,\"fare\":78903200282,\"fare_basis_code\":\"4bya5wMZ9Z\",\"origin_country\":\"s0dfCHCOCa\"}],\"passengers\":[{\"pax_id\":\"rjX3KvdmdR\",\"pax_type\":\"DNtJOL1hWI\",\"first_name\":\"Nova\",\"last_name\":\"Adrian\",\"title\":\"Mrs\",\"gender\":\"M\",\"date_of_birth\":\"20220420\",\"identity_number\":\"6dJSKHst0j\",\"name_in_pnr\":\"Q6cI88XO0s\",\"member_ticket\":\"4huqVGMa7Q\"}]},\"billing\":{\"country_code\":null,\"state_provine\":null,\"city_name\":null,\"postal_code\":null,\"street_number\":null,\"address_line1\":null,\"address_line2\":null},\"shipping\":{\"country_code\":null,\"state_provine\":null,\"city_name\":null,\"postal_code\":null,\"street_number\":null,\"address_line1\":null,\"address_line2\":null}}";

            ExtraData extra = new ExtraData();
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
                //extraData = JsonConvert.DeserializeObject<object>(extraJson),
                signature = ""
            };

            // UAT
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
