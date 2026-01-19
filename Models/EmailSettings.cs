// 17/01/2026 - 22:31:56
// DANGTHUY

namespace LushEnglishAPI.Models;

public class EmailSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }

    public string FromEmail { get; set; }
    public string FromName { get; set; }

    public string Username { get; set; }
    public string Password { get; set; }
}
