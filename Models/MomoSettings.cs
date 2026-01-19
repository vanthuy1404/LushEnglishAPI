// 14/01/2026 - 22:02:55
// DANGTHUY

namespace LushEnglishAPI.Models;


public class MomoSettings
{
    public string Endpoint { get; set; } = "";
    public string PartnerCode { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";

    public string RedirectUrl { get; set; } = ""; // FE url
    public string IpnUrl { get; set; } = "";      // BE public url (ngrok)
}