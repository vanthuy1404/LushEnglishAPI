// 19/11/2025 - 21:33:10
// DANGTHUY

using AutoMapper;
using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabularyController(LushEnglishDbContext context, IMapper mapper) : ControllerBase
{
    // GET: api/Vocabulary
    [HttpGet]
    public async Task<ActionResult<List<VocabularyDTO>>> GetAllVocabularies()
    {
        var vocabularies = await context.Vocabularies.ToListAsync();
        var results = mapper.Map<List<VocabularyDTO>>(vocabularies).OrderByDescending(x => x.CreatedAt).ToList();
        return Ok(results);
    }

    // GET: api/Vocabulary/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<VocabularyDTO?>> GetVocabulary(Guid id)
    {
        var vocab = await context.Vocabularies.FindAsync(id);
        if (vocab == null)
            return NotFound("Vocabulary not found");

        var result = mapper.Map<VocabularyDTO>(vocab);
        return Ok(result);
    }

    // POST: api/Vocabulary
    [HttpPost]
    [AdminCheck]
    public async Task<ActionResult<VocabularyDTO>> CreateVocabulary([FromBody] VocabularyDTO dto)
    {
        var vocab = new Vocabulary
        {
            Id = Guid.NewGuid(),
            TopicId = dto.TopicId,
            Word = dto.Word,
            Phonetic = dto.Phonetic,
            Meaning = dto.Meaning,
            CreatedAt = DateTime.UtcNow.AddHours(7)
        };

        await context.Vocabularies.AddAsync(vocab);
        await context.SaveChangesAsync();

        var result = mapper.Map<VocabularyDTO>(vocab);
        return Ok(result);
    }

    // PUT: api/Vocabulary/{id}
    [HttpPut("{id}")]
    [AdminCheck]
    public async Task<ActionResult<VocabularyDTO>> UpdateVocabulary(Guid id, [FromBody] VocabularyDTO dto)
    {
        var vocab = await context.Vocabularies.FindAsync(id);
        if (vocab == null)
            return NotFound("Vocabulary not found");

        vocab.Word = dto.Word;
        vocab.Phonetic = dto.Phonetic;
        vocab.Meaning = dto.Meaning;

        context.Vocabularies.Update(vocab);
        await context.SaveChangesAsync();

        var result = mapper.Map<VocabularyDTO>(vocab);
        return Ok(result);
    }

    // DELETE: api/Vocabulary/{id}
    [HttpDelete("{id}")]
    [AdminCheck]
    public async Task<IActionResult> DeleteVocabulary(Guid id)
    {
        var vocab = await context.Vocabularies.FindAsync(id);
        if (vocab == null)
            return NotFound("Vocabulary not found");

        context.Vocabularies.Remove(vocab);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
