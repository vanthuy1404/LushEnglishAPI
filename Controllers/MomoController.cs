// 14/01/2026 - 22:04:41
// DANGTHUY
using System.Security.Cryptography;
using System.Text;
using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MomoController(
    LushEnglishDbContext context,
    IOptions<MomoSettings> momoOptions,
    IHttpClientFactory httpClientFactory
) : ControllerBase
{
    private readonly LushEnglishDbContext _context = context;
    private readonly MomoSettings _momo = momoOptions.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    // ============ CREATE PAYMENT ============
    // FE gọi: POST /api/momo/create  body: { courseId }
    [HttpPost("create")]
    [SessionCheck]
    public async Task<IActionResult> CreatePayment([FromBody] CreateMomoPaymentDTO dto)
    {
        // Lấy userId từ header (SessionCheck đã validate)
        var userIdHeader = Request.Headers["UserId"].ToString();
        if (!Guid.TryParse(userIdHeader, out var userId))
            return Unauthorized("Missing or invalid UserId header");

        var course = await _context.Courses.FirstOrDefaultAsync(x => x.Id == dto.CourseId);
        if (course == null) return NotFound("Course not found");

        // (Optional) chặn mua trùng nếu đã PAID
        var existedPaid = await _context.UserCourses.AnyAsync(x =>
            x.UserId == userId && x.CourseId == course.Id && x.Status == "PAID");
        if (existedPaid) return BadRequest("You already purchased this course.");

        // Amount tại thời điểm mua
        decimal finalAmount = course.Price;
        if (course.Discount.HasValue && course.Discount.Value > 0)
            finalAmount = course.Price - (course.Price * course.Discount.Value / 100m);

        if (finalAmount < 0) finalAmount = 0;

        string endpoint = _momo.Endpoint;
        string partnerCode = _momo.PartnerCode;
        string accessKey = _momo.AccessKey;
        string secretKey = _momo.SecretKey;

        string orderId = Guid.NewGuid().ToString(); // unique
        string requestId = Guid.NewGuid().ToString();
        string orderInfo = $"Thanh toán khóa học {course.Name}";
        string redirectUrl = _momo.RedirectUrl; // FE
        string ipnUrl = _momo.IpnUrl;           // BE public
        string amount = ((long)finalAmount).ToString(); // MoMo thường nhận int string
        string requestType = "captureWallet";

        // Lưu UserCourse PENDING trước
        var userCourse = new UserCourse
        {
            UserId = userId,
            CourseId = course.Id,
            Amount = finalAmount,
            Status = "PENDING",
            MomoOrderId = orderId,
            MomoRequestId = requestId,
            CreatedAt = DateTime.UtcNow.AddHours(7),
        };
        _context.UserCourses.Add(userCourse);
        await _context.SaveChangesAsync();

        // Tạo chữ ký (signature) y hệt code bạn gửi
        string rawHash =
            $"accessKey={accessKey}&amount={amount}&extraData=&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";

        string signature;
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHash));
            signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        var message = new
        {
            partnerCode,
            accessKey,     // ✅ có accessKey (bạn code fashionstore không gửi accessKey vào body, nhưng rawHash có; gửi luôn cho chắc)
            requestId,
            amount,
            orderId,
            orderInfo,
            redirectUrl,
            ipnUrl,
            requestType,
            extraData = "",
            signature,
            lang = "vi"
        };

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsync(endpoint,
            new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json"));

        var result = await response.Content.ReadAsStringAsync();

        // Trả raw/parsed đều ok, FE chỉ cần payUrl
        return Ok(result);
    }

    // ============ IPN ============
    // MoMo gọi server-to-server
    [HttpPost("ipn")]
public async Task<IActionResult> PaymentNotify()
{
    // ✅ đọc raw body
    using var reader = new StreamReader(Request.Body);
    var requestBody = await reader.ReadToEndAsync();

    Console.WriteLine("===== MOMO IPN HIT =====");
    Console.WriteLine($"Time: {DateTime.UtcNow.AddHours(7):yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"Raw body: {requestBody}");

    if (string.IsNullOrWhiteSpace(requestBody))
        return Ok(new { RspCode = "01", Message = "Empty request" });

    MomoIPNResponse? notifyData = null;

    try
    {
        notifyData = JsonConvert.DeserializeObject<MomoIPNResponse>(requestBody);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Deserialize error: {ex.Message}");
        return Ok(new { RspCode = "01", Message = "Deserialize error" });
    }

    if (notifyData == null)
        return Ok(new { RspCode = "01", Message = "Invalid body" });

    var orderId = (notifyData.OrderId ?? "").Trim();
    Console.WriteLine($"Parsed orderId: '{orderId}' | resultCode: {notifyData.ResultCode} | transId: {notifyData.TransId}");

    if (string.IsNullOrWhiteSpace(orderId))
        return Ok(new { RspCode = "01", Message = "Missing orderId" });

    // ✅ tìm record theo orderId
    var uc = await _context.UserCourses.FirstOrDefaultAsync(x => x.MomoOrderId == orderId);

    if (uc == null)
    {
        Console.WriteLine("❌ Order not found in DB for orderId = " + orderId);
        return Ok(new { RspCode = "01", Message = "Order not found" });
    }

    Console.WriteLine($"Found UserCourse: {uc.Id} | Status before: {uc.Status}");

    // ✅ idempotent
    if (uc.Status == "PAID")
    {
        Console.WriteLine("Already PAID - ignore");
        return Ok(new { RspCode = "00", Message = "Already processed" });
    }

    // TODO: verify signature (khuyến nghị)
    // if (!VerifyMomoSignature(notifyData, requestBody)) return Ok(new { RspCode="01", Message="Invalid signature" });

    if (notifyData.ResultCode == 0)
    {
        uc.Status = "PAID";
        uc.MomoTransId = notifyData.TransId?.ToString();
        uc.PaidAt = DateTime.UtcNow.AddHours(7);
    }
    else
    {
        uc.Status = "FAILED";
    }

    await _context.SaveChangesAsync();

    Console.WriteLine($"✅ Updated Status: {uc.Status} | PaidAt: {uc.PaidAt}");
    return Ok(new { RspCode = "00", Message = "Success" });
}

}

// ===== DTOs for MoMo =====
public class CreateMomoPaymentDTO
{
    public Guid CourseId { get; set; }
}

// Model nhận IPN (tối thiểu)
public class MomoIPNResponse
{
    [JsonProperty("orderId")]
    public string? OrderId { get; set; }

    [JsonProperty("resultCode")]
    public int ResultCode { get; set; }

    [JsonProperty("transId")]
    public long? TransId { get; set; }

    [JsonProperty("signature")]
    public string? Signature { get; set; }
}