// 17/01/2026 - 23:13:53
// DANGTHUY

using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailCampaignController : ControllerBase
{
    private readonly LushEnglishDbContext _db;

    public EmailCampaignController(LushEnglishDbContext db)
    {
        _db = db;
    }

    // GET: api/EmailCampaign
    [HttpGet]
    public async Task<ActionResult<List<EmailCampaign>>> GetAll()
    {
        var items = await _db.EmailCampaigns
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(items);
    }

    // GET: api/EmailCampaign/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmailCampaign>> GetById(Guid id)
    {
        var item = await _db.EmailCampaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null) return NotFound("EmailCampaign not found");
        return Ok(item);
    }

    // POST: api/EmailCampaign
    [HttpPost]
    public async Task<ActionResult<EmailCampaign>> Create([FromBody] CreateEmailCampaignDTO dto)
    {
        var now = DateTime.UtcNow.AddHours(7);
        var today = now.Date;

        var validationError = ValidateCampaign(dto.StartDate, dto.EndDate, dto.SendTimeOfDay, dto.IsEnabled, now);
        if (validationError != null) return BadRequest(validationError);

        var entity = new EmailCampaign
        {
            Id = Guid.CreateVersion7(),
            Name = dto.Name.Trim(),
            Subject = dto.Subject.Trim(),
            HtmlBody = dto.HtmlBody,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            SendTimeOfDay = dto.SendTimeOfDay,
            IsEnabled = dto.IsEnabled,
            CreatedAt = now,
            UpdatedAt = null
        };

        _db.EmailCampaigns.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(entity);
    }

    // PUT: api/EmailCampaign/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmailCampaign>> Update(Guid id, [FromBody] UpdateEmailCampaignDTO dto)
    {
        var now = DateTime.UtcNow.AddHours(7);

        var entity = await _db.EmailCampaigns.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound("EmailCampaign not found");

        var validationError = ValidateCampaign(dto.StartDate, dto.EndDate, dto.SendTimeOfDay, dto.IsEnabled, now);
        if (validationError != null) return BadRequest(validationError);

        entity.Name = dto.Name.Trim();
        entity.Subject = dto.Subject.Trim();
        entity.HtmlBody = dto.HtmlBody;

        entity.StartDate = dto.StartDate.Date;
        entity.EndDate = dto.EndDate.Date;
        entity.SendTimeOfDay = dto.SendTimeOfDay;

        entity.IsEnabled = dto.IsEnabled;
        entity.UpdatedAt = now;

        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    // DELETE: api/EmailCampaign/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.EmailCampaigns.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound("EmailCampaign not found");

        // nếu muốn xóa delivery log khi xóa campaign:
        var deliveries = await _db.EmailCampaignDeliveries
            .Where(x => x.CampaignId == id)
            .ToListAsync();

        if (deliveries.Count > 0)
            _db.EmailCampaignDeliveries.RemoveRange(deliveries);

        _db.EmailCampaigns.Remove(entity);
        await _db.SaveChangesAsync();

        return Ok("Deleted");
    }

    /// <summary>
    /// Validate theo yêu cầu:
    /// - StartDate >= today
    /// - EndDate >= StartDate
    /// - Nếu StartDate là hôm nay => SendTimeOfDay >= now + 1 giờ
    /// - Nếu IsEnabled=true => today phải nằm trong [StartDate..EndDate] và EndDate không được < today
    /// </summary>
    private static string? ValidateCampaign(
        DateTime startDateInput,
        DateTime endDateInput,
        TimeSpan sendTimeOfDay,
        bool isEnabled,
        DateTime nowVn)
    {
        var today = nowVn.Date;
        var startDate = startDateInput.Date;
        var endDate = endDateInput.Date;
        

        //  FIX: bật active cho phép cả campaign tương lai
        if (isEnabled)
        {
            // chỉ cấm nếu đã quá hạn
            if (endDate < today)
                return "Cannot activate: campaign is already expired (EndDate < today).";
        }

        return null;
    }
}