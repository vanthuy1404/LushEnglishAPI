// 17/01/2026 - 22:33:43
// DANGTHUY

using LushEnglishAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/test")]
public class TestEmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public TestEmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-mail")]
    public async Task<IActionResult> SendTestMail()
    {
        await _emailService.SendAsync(
            "tranphuongxinh27@gmail.com",
            "SMTP Test",
            "<h2>Email system is working 🚀</h2>"
        );

        return Ok("Email sent");
    }
}
