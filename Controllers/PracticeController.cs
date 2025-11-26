// 26/11/2025 - 22:00:10
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
public class PracticeController(LushEnglishDbContext context, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Practice>>> GetPractices()
    {
        var practices = await context.Practices.ToListAsync();
        var result = mapper.Map<List<PracticeDTO>>(practices);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Practice>> GetPractice(Guid id)
    {
        var practice = await context.Practices.FindAsync(id);
        if (practice == null)
        {
            return NotFound("Practice not found");
        }
        var result = mapper.Map<PracticeDTO>(practice);
        var questions = await context.Questions.Where(q => q.PracticeId == id).ToListAsync();
        var questionDtos = mapper.Map<List<QuestionDTO>>(questions);
        foreach (var question in questionDtos)
        {
            question.CorrectOption = "";
        }
        result.Questions = questionDtos;
        return Ok(result);
    }
    [HttpPost]
    public async Task<ActionResult<PracticeDTO>> CreatePractice([FromBody] PracticeDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var practice = new Practice
        {
            Id = Guid.CreateVersion7(),   // Tạo ID backend
            TopicId = dto.TopicId,
            Name = dto.Name,
            PracticeType = dto.PracticeType,
            CreatedAt = DateTime.UtcNow
        };

        context.Practices.Add(practice);
        await context.SaveChangesAsync();

        return Ok(practice);
    }
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdatePractice(Guid id, [FromBody] PracticeDTO dto)
    {
        var practice = await context.Practices.FindAsync(id);
        if (practice == null)
            return NotFound("Practice not found");

        // Update fields
        practice.TopicId = dto.TopicId;
        practice.Name = dto.Name;
        practice.PracticeType = dto.PracticeType;

        await context.SaveChangesAsync();
        return Ok(practice);
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePractice(Guid id)
    {
        var practice = await context.Practices.FindAsync(id);
        if (practice == null)
            return NotFound("Practice not found");

        context.Practices.Remove(practice);
        await context.SaveChangesAsync();
        return Ok("Deleted successfully");
    }

}