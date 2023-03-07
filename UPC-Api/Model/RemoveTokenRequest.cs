using System.ComponentModel;

namespace UPC.Api.Model;
#pragma warning disable CS8618

public class RemoveTokenRequest
{
    [DefaultValue("000164B15B5D0CA64CD1A6D34D6C55B8683A")]
    [SwaggerValue("000164B15B5D0CA64CD1A6D34D6C55B8683A")]
    public string Token { get; set; }
}