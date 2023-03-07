using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

#pragma warning disable CS8618
namespace UPC.Api.Model
{
    public class PayOrderRequest
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

        [DefaultValue("INTERNATIONAL, DOMESTIC, WALLET, HUB, QRPAY, ..")]
        [SwaggerValue("WALLET")]
        public string PaymentMethod { get; set; } 
        
        [DefaultValue("MOMO, ZALO, WALLET, GPAY, 2C2P, POLI, QRPAY, ..")]
        [SwaggerValue("MOMO")]
        public string? SourceType { get; set; } 
        
        [DefaultValue("{}")]
        [SwaggerValue("{}")]
        public object ExtraData { get; set; }
        
        [DefaultValue("vi, en")]
        [SwaggerValue("vi")]
        public string Language { get; set; }

        #region Hosted Merchant Only
        [DefaultValue("4012000033330026")]
        [SwaggerValue(" ")]
        public string? CardNumber { get; set; } = default!;
        
        [DefaultValue("NGUYEN VAN A")]
        [SwaggerValue(" ")]
        public string? CardHolderName { get; set; } = default!;
        
        [DefaultValue("100")]
        [SwaggerValue(" ")]
        public string? CardExpireDate { get; set; } = default!;
        
        [DefaultValue("01/39")]
        [SwaggerValue(" ")]
        public string? CardVerificationValue { get; set; } = default!;
        #endregion
		
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
}


[AttributeUsage(validOn: AttributeTargets.Property)]
public class SwaggerValueAttribute : Attribute
{
    public SwaggerValueAttribute(object value)
    {
        Value = value;
    }

    public SwaggerValueAttribute(Type type)
    {
        Type = type;
    }

    public SwaggerValueAttribute(Type type, params object[] parameters)
    {
        Type = type;
        Parameters = parameters;
    }

    public Type? Type { get; }
    public object Value { get; } = default!;
    public object?[] Parameters { get; } = Array.Empty<object?>();
}

public class SwaggerDefaultValueAttribute: Attribute
{
    public SwaggerDefaultValueAttribute(string value)
    {
        Value = value;
    }
    public string Parameter {get; set;}
    public string Value {get; set;}
}

public class SwaggerFilter : ISchemaFilter
{
    private static readonly MethodInfo MethodCreateInstance = GetActivatorMethod();


    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
        {
            return;
        }

        foreach (PropertyInfo propertyInfo in context.Type.GetProperties())
        {
            SwaggerValueAttribute? attribute = propertyInfo.GetCustomAttribute<SwaggerValueAttribute>();
            if (attribute == null)
            {
                continue;
            }

            foreach (KeyValuePair<string, OpenApiSchema> property in schema.Properties)
            {
                if (propertyInfo.Name.Equals(property.Key, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                SetPropertyValue(attribute, property);
                break;
            }
        }
    }

    private static MethodInfo GetActivatorMethod()
    {
        return typeof(Activator)
            .GetMethods()
            .Single(method =>
                method.Name == nameof(Activator.CreateInstance) &&
                method.IsGenericMethod &&
                method.GetGenericArguments().Length == 1);
    }

    private static object GetRuntimeValue(Type instanceType, SwaggerValueAttribute attribute)
    {
        Type swaggerType = typeof(ISwagger<>);
        Type interfaceType = instanceType.GetInterface(swaggerType.Name)!;
        Type genericType = swaggerType.MakeGenericType(interfaceType.GetGenericArguments()[0]);
        MethodInfo methodGenerate = genericType.GetMethod(nameof(ISwagger<object>.Generate))!;
        MethodInfo genericMethod = MethodCreateInstance.MakeGenericMethod(instanceType);
        object? instance = genericMethod.Invoke(null, Array.Empty<object?>());
        return methodGenerate.Invoke(instance, new object?[] { attribute.Parameters })!;
    }

    private static void SetPropertyValue(
        SwaggerValueAttribute attribute,
        KeyValuePair<string, OpenApiSchema> property)
    {
        Type? instanceType = attribute.Type;
        object value = instanceType != null ? GetRuntimeValue(instanceType, attribute) : attribute.Value;

        property.Value.Example = Type.GetTypeCode(value.GetType()) switch
        {
            TypeCode.Boolean => new OpenApiBoolean((bool)value),
            TypeCode.Int16 or TypeCode.Int32 => new OpenApiInteger((int)value),
            TypeCode.Int64 => new OpenApiLong((long)value),
            TypeCode.Single => new OpenApiFloat((float)value),
            TypeCode.Double or TypeCode.Decimal => new OpenApiDouble((double)value),
            TypeCode.String => new OpenApiString((string)value),
            _ => property.Value.Example
        };
    }
}

internal interface ISwagger<out T>
{
    T Generate(params object?[] parameters);
}

public class GuidSwagger : ISwagger<string>
{
    public string Generate(params object?[] parameters)
    {
        return Guid.NewGuid().ToString("N");
    }
}

public class DateTimeStringSwagger : ISwagger<string>
{
    public string Generate(params object?[] parameters)
    {
        return DateTime.Now.ToString("yyyyMMddHHmmss");
    }
}
