namespace SRGS.infrastructure.Identity.LdapOptions;

public sealed class LdapSettings
{
    public string Server { get; set; } = string.Empty;

    public int Port { get; set; } = 636;

    public string BaseDn { get; set; } = string.Empty;

    public bool UseSsl { get; set; } = true;

    public string BindUsername { get; set; } = string.Empty;

    public string BindPassword { get; set; } = string.Empty;
}
