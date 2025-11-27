// 27/11/2025 - 22:00:00
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
public class ChattingController(LushEnglishDbContext context, IMapper mapper) : ControllerBase
{
    // GET: api/Chatting
    [HttpGet]
    public async Task<ActionResult<List<ChattingExerciseDTO>>> GetChattingConfigs()
    {
        var chattingConfigs = await context.ChattingConfigs.ToListAsync();
        
        // Sử dụng AutoMapper để map từ Entity sang DTO
        var result = mapper.Map<List<ChattingExerciseDTO>>(chattingConfigs);
        return Ok(result);
    }

    // GET: api/Chatting/5a70d10c-f37c-4a3e-b873-1f1966a3311e
    [HttpGet("{id}")]
    public async Task<ActionResult<ChattingExerciseDTO>> GetChattingConfig(Guid id)
    {
        var chattingConfig = await context.ChattingConfigs.FindAsync(id);
        
        if (chattingConfig == null)
        {
            return NotFound("Chatting Configuration not found");
        }
        
        // Sử dụng AutoMapper để map từ Entity sang DTO
        var result = mapper.Map<ChattingExerciseDTO>(chattingConfig);
        return Ok(result);
    }

    // POST: api/Chatting
    [HttpPost]
    public async Task<ActionResult<ChattingExerciseDTO>> CreateChattingConfig([FromBody] ChattingExerciseDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Ánh xạ thủ công (Manual mapping) từ DTO sang Entity (ChattingConfig)
        var chattingConfig = new ChattingConfig
        {
            Id = Guid.NewGuid(),   
            TopicId = dto.TopicId, // Đã sửa để dùng TopicId từ DTO
            Name = dto.Name,
            AiRole = dto.AiRole,
            Requirement = dto.Requirement, 
            Objective = dto.Objective, 
            MaxAiReplies = dto.MaxAiReplies,
            OpeningQuestion = dto.OpeningQuestion,
            Level = dto.Level,
            LinkImage = dto.LinkImage,
            CreatedAt = DateTime.UtcNow 
        };

        context.ChattingConfigs.Add(chattingConfig);
        await context.SaveChangesAsync();

        // Map Entity đã lưu trở lại DTO để trả về
        var result = mapper.Map<ChattingExerciseDTO>(chattingConfig);
        return CreatedAtAction(nameof(GetChattingConfig), new { id = chattingConfig.Id }, result);
    }
    
    // PUT: api/Chatting/5a70d10c-f37c-4a3e-b873-1f1966a3311e
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateChattingConfig(Guid id, [FromBody] ChattingExerciseDTO dto)
    {
        var chattingConfig = await context.ChattingConfigs.FindAsync(id);
        
        if (chattingConfig == null)
            return NotFound("Chatting Configuration not found");
            
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Ánh xạ thủ công (Manual mapping) các trường từ DTO sang Entity
        chattingConfig.TopicId = dto.TopicId; // Đã sửa để dùng TopicId từ DTO
        chattingConfig.Name = dto.Name;
        chattingConfig.AiRole = dto.AiRole;
        chattingConfig.Requirement = dto.Requirement;
        chattingConfig.Objective = dto.Objective;
        chattingConfig.MaxAiReplies = dto.MaxAiReplies;
        chattingConfig.OpeningQuestion = dto.OpeningQuestion;
        chattingConfig.Level = dto.Level;
        chattingConfig.LinkImage = dto.LinkImage;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.ChattingConfigs.Any(e => e.Id == id))
            {
                return NotFound("Chatting Configuration not found during update check.");
            }
            throw;
        }

        return NoContent(); // 204 No Content cho PUT thành công
    }

    // DELETE: api/Chatting/5a70d10c-f37c-4a3e-b873-1f1966a3311e
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteChattingConfig(Guid id)
    {
        var chattingConfig = await context.ChattingConfigs.FindAsync(id);
        
        if (chattingConfig == null)
            return NotFound("Chatting Configuration not found");

        context.ChattingConfigs.Remove(chattingConfig);
        await context.SaveChangesAsync();
        
        return Ok("Chatting Configuration deleted successfully");
    }
}