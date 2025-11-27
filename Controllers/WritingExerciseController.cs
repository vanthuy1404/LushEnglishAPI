// 27/11/2025 - 10:01:49
// DANGTHUY

using AutoMapper;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WritingController(LushEnglishDbContext context, IMapper mapper) : ControllerBase
{
    // GET: api/Writing
    [HttpGet]
    public async Task<ActionResult<List<WritingExerciseDTO>>> GetWritingExercises()
    {
        // Giả định bảng/entity tương ứng với WritingExerciseDTO là WritingConfigs
        var writingConfigs = await context.WritingConfigs.ToListAsync();
        
        // Sử dụng AutoMapper để map từ Entity sang DTO
        var result = mapper.Map<List<WritingExerciseDTO>>(writingConfigs);
        return Ok(result);
    }

    // GET: api/Writing/5a70d10c-f37c-4a3e-b873-1f1966a3311e
    [HttpGet("{id}")]
    public async Task<ActionResult<WritingExerciseDTO>> GetWritingExercise(Guid id)
    {
        var writingConfig = await context.WritingConfigs.FindAsync(id);
        
        if (writingConfig == null)
        {
            return NotFound("Writing Exercise not found");
        }
        
        // Sử dụng AutoMapper để map từ Entity sang DTO
        var result = mapper.Map<WritingExerciseDTO>(writingConfig);
        return Ok(result);
    }

    // POST: api/Writing
    [HttpPost]
    public async Task<ActionResult<WritingExerciseDTO>> CreateWritingExercise([FromBody] WritingExerciseDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Ánh xạ thủ công (Manual mapping) từ DTO sang Entity (WritingConfigs)
        var writingConfig = new WritingConfig
        {
            Id = Guid.NewGuid(),   // Tạo ID mới cho Entity
            TopicId = dto.TopicId,
            Name = dto.Name,
            Requirement = dto.Requirement, // Requirement là chuỗi JSON
            Level = dto.Level,
            LinkImage = dto.LinkImage,
            CreatedAt = DateTime.UtcNow
        };

        context.WritingConfigs.Add(writingConfig);
        await context.SaveChangesAsync();

        // Map Entity đã lưu trở lại DTO để trả về
        var result = mapper.Map<WritingExerciseDTO>(writingConfig);
        return CreatedAtAction(nameof(GetWritingExercise), new { id = writingConfig.Id }, result);
    }
    
    // PUT: api/Writing/5a70d10c-f37c-4a3e-b873-1f1966a3311e
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateWritingExercise(Guid id, [FromBody] WritingExerciseDTO dto)
    {
        var writingConfig = await context.WritingConfigs.FindAsync(id);
        
        if (writingConfig == null)
            return NotFound("Writing Exercise not found");

        // Ánh xạ thủ công (Manual mapping) các trường từ DTO sang Entity
        writingConfig.TopicId = dto.TopicId;
        writingConfig.Name = dto.Name;
        writingConfig.Requirement = dto.Requirement;
        writingConfig.Level = dto.Level;
        writingConfig.LinkImage = dto.LinkImage;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.WritingConfigs.Any(e => e.Id == id))
            {
                return NotFound("Writing Exercise not found during update check.");
            }
            throw;
        }

        return NoContent(); // Trả về 204 No Content cho PUT thành công
    }

    // DELETE: api/Writing/5a70d10c-f37c-4a3e-b873-1f1966a3311e
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWritingExercise(Guid id)
    {
        var writingConfig = await context.WritingConfigs.FindAsync(id);
        
        if (writingConfig == null)
            return NotFound("Writing Exercise not found");

        context.WritingConfigs.Remove(writingConfig);
        await context.SaveChangesAsync();
        
        return Ok("Writing Exercise deleted successfully");
    }
}