using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OracleFusion.GetEssJobStatus.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// Oracle Fusion instance base URL.
    /// </summary>
    /// <example>https://example.oraclecloud.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("https://example.oraclecloud.com")]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Oracle Fusion username.
    /// </summary>
    /// <example>john.doe@example.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Oracle Fusion password.
    /// </summary>
    /// <example>MySecretPassword123</example>
    [PasswordPropertyText]
    [DefaultValue("")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// API version for the fscmRestApi endpoint.
    /// </summary>
    /// <example>latest OR xx.xx.xx.xx</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("latest")]
    public string ApiVersion { get; set; } = "latest";
}
