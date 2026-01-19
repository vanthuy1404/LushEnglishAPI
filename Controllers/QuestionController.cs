// 09/12/2025 - DANGTHUY
// Question Controller for CRUD operations

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models; // Giả định đây là namespace chứa Model Question
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionController(LushEnglishDbContext context, IMapper mapper) : ControllerBase
{
    private readonly LushEnglishDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    // --- GET ALL: api/Question (Chỉ dùng cho Admin để lấy danh sách chi tiết) ---
    [HttpGet]
    public async Task<ActionResult<List<QuestionDTO>>> GetAllQuestions()
    {
        var questions = await _context.Questions.ToListAsync();
        var results = _mapper.Map<List<QuestionDTO>>(questions);
        return Ok(results);
    }
    
    // --- GET BY ID: api/Question/{id} ---
    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionDTO>> GetQuestion(Guid id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return NotFound("Question not found.");

        var result = _mapper.Map<QuestionDTO>(question);
        return Ok(result);
    }
    
    // --- GET BY PRACTICE ID: api/Question/ByPractice/{practiceId} ---
    // Endpoint tiện lợi để lấy tất cả câu hỏi của một Practice cụ thể
    [HttpGet("ByPractice/{practiceId}")]
    public async Task<ActionResult<List<QuestionDTO>>> GetQuestionsByPracticeId(Guid practiceId)
    {
        var questions = await _context.Questions
            .Where(q => q.PracticeId == practiceId)
            .ToListAsync();

        var results = _mapper.Map<List<QuestionDTO>>(questions);
        return Ok(results);
    }

    // --- CREATE: api/Question ---
    [HttpPost]
    [AdminCheck]
    public async Task<ActionResult<QuestionDTO>> CreateQuestion([FromBody] QuestionDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Kiểm tra PracticeId có tồn tại không
        var practiceExists = await _context.Practices.AnyAsync(p => p.Id == dto.PracticeId);
        if (!practiceExists)
            return NotFound("Parent Practice not found.");

        // 2. Map DTO sang Model (Entity)
        var question = _mapper.Map<Question>(dto);
        
        // Gán ID mới và đảm bảo CreatedAt (nếu có trong Model)
        question.Id = Guid.CreateVersion7(); 
        // Nếu Model Question có trường CreatedAt, bạn cần gán giá trị ở đây

        // 3. Lưu vào DB
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // 4. Map lại sang DTO để trả về (có kèm ID mới)
        var result = _mapper.Map<QuestionDTO>(question);
        return CreatedAtAction(nameof(GetQuestion), new { id = result.Id }, result);
    }

    // --- UPDATE: api/Question/{id} ---
    [HttpPut("{id}")]
    [AdminCheck]
    public async Task<ActionResult> UpdateQuestion(Guid id, [FromBody] QuestionDTO dto)
    {
        if (id != dto.Id)
        {
            // Nếu DTO không gửi ID hoặc ID không khớp với route
            return BadRequest("ID mismatch.");
        }
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return NotFound("Question not found.");

        // 1. Kiểm tra PracticeId (đảm bảo không đổi sang Practice không tồn tại)
        var practiceExists = await _context.Practices.AnyAsync(p => p.Id == dto.PracticeId);
        if (!practiceExists)
            return NotFound("Parent Practice not found.");

        // 2. Map DTO lên entity (AutoMapper sẽ update các thuộc tính)
        _mapper.Map(dto, question); 
        // Lưu ý: Nếu có trường CreatedAt trong Model, không nên overwrite nó

        // 3. Lưu thay đổi
        _context.Entry(question).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Questions.Any(e => e.Id == id))
            {
                return NotFound("Question not found after update check.");
            }
            throw;
        }

        return NoContent(); // 204 No Content là chuẩn cho PUT thành công
    }

    // --- DELETE: api/Question/{id} ---
    [HttpDelete("{id}")]
    [AdminCheck]
    public async Task<ActionResult> DeleteQuestion(Guid id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return NotFound("Question not found.");

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();

        return Ok("Question deleted successfully.");
    }
}