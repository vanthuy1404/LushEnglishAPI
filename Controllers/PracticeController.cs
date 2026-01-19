// 26/11/2025 - 22:00:10 (Updated to support IFormFile upload)
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;
using LushEnglishAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PracticeController(
    LushEnglishDbContext context, 
    IMapper mapper,
    // Thêm IWebHostEnvironment để truy cập wwwroot
    IWebHostEnvironment env,
    TopicIdsService _topicIdsService
) : ControllerBase
{
    private readonly LushEnglishDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IWebHostEnvironment _env = env;

    // --- GET (Giữ nguyên logic cũ) ---
    
    [HttpGet]
    [SessionCheck]
    public async Task<ActionResult<List<PracticeDTO>>> GetPractices()
    {
        var practices = await _context.Practices
            .OrderByDescending(x => x.CreatedAt) // Sắp xếp trên DB
            .ToListAsync();  
        var results = _mapper.Map<List<PracticeDTO>>(practices);
        foreach (var result in results)
        {
            // Lấy TopicName
            var topic = await _context.Topics.Where(t => t.Id == result.TopicId).FirstOrDefaultAsync();
            result.TopicName = topic?.Name;
            
            // Nếu Practice không có ảnh riêng, dùng ảnh của Topic
            if (String.IsNullOrEmpty(result.LinkImage))
            {
                result.LinkImage = topic?.LinkImage ?? "";
            }

            var questions = await _context.Questions.Where(q => q.PracticeId == result.Id).ToListAsync();
            if (questions != null && questions.Count > 0)
            {
                result.NumberOfQuestions = questions.Count;
                result.TimeDuration = questions.Count * 2;
            }
            else
            {
                result.NumberOfQuestions = 0;
                result.TimeDuration = 0;
            }

        }

        return Ok(results);
    }
    
    [HttpGet("my-practices")]
    [SessionCheck]
    public async Task<ActionResult<List<PracticeDTO>>> GetMyPractices()
    {
        List<Guid> topicIds;

        try
        {
            topicIds = await _topicIdsService.GetMyTopicIdsAsync();
        }
        catch (UnauthorizedAccessException)
        {
            return Ok(new List<PracticeDTO>());
        }

        if (topicIds.Count == 0)
            return Ok(new List<PracticeDTO>());

        var practices = await _context.Practices
            .Where(p => topicIds.Contains(p.TopicId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var results = _mapper.Map<List<PracticeDTO>>(practices);

        // ===== Bulk load topic for TopicName + fallback image =====
        var topicIdList = results.Select(x => x.TopicId).Distinct().ToList();

        var topicMap = await _context.Topics
            .Where(t => topicIdList.Contains(t.Id))
            .Select(t => new { t.Id, t.Name, t.LinkImage })
            .ToDictionaryAsync(x => x.Id, x => x);

        // ===== Bulk count questions to avoid N+1 =====
        var practiceIds = results.Select(x => x.Id).ToList();

        var questionCounts = await _context.Questions
            .Where(q => practiceIds.Contains(q.PracticeId))
            .GroupBy(q => q.PracticeId)                 // ✅ Guid
            .Select(g => new { PracticeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PracticeId, x => x.Count);
        foreach (var r in results)
        {
            if (topicMap.TryGetValue(r.TopicId, out var t))
            {
                r.TopicName = t.Name;

                if (string.IsNullOrEmpty(r.LinkImage))
                    r.LinkImage = t.LinkImage ?? "";
            }

            var count = (r.Id.HasValue && questionCounts.TryGetValue(r.Id.Value, out var c)) ? c : 0;
            r.NumberOfQuestions = count;
            r.TimeDuration = count * 2;
        }

        return Ok(results);
    }

    [HttpGet("{id}")]
    [SessionCheck]
    public async Task<ActionResult<Practice>> GetPractice(Guid id)
    {
        var practice = await _context.Practices.FindAsync(id);
        if (practice == null)
        {
            return NotFound("Practice not found");
        }

        var result = _mapper.Map<PracticeDTO>(practice);
        var topic = await _context.Topics.Where(t => t.Id == result.TopicId).FirstOrDefaultAsync();
        result.TopicName = topic?.Name;
        
        if (String.IsNullOrEmpty(result.LinkImage))
        {
            result.LinkImage = topic?.LinkImage ?? "";
        }

        var questions = await _context.Questions.Where(q => q.PracticeId == id).ToListAsync();
        if (questions != null && questions.Count > 0)
        {
            result.NumberOfQuestions = questions.Count;
            result.TimeDuration = questions.Count * 2;
        }
        else
        {
            result.NumberOfQuestions = 0;
            result.TimeDuration = 0;
        }
        var questionDtos = _mapper.Map<List<QuestionDTO>>(questions);
        foreach (var question in questionDtos)
        {
            question.CorrectOption = ""; // Không trả về đáp án đúng cho người dùng cuối
        }

        result.Questions = questionDtos;
        return Ok(result);
    }
    [HttpGet("admin/{id}")]
    [SessionCheck]
    [AdminCheck]
    public async Task<ActionResult<Practice>> GetPracticeForAdmin(Guid id)
    {
        var practice = await _context.Practices.FindAsync(id);
        if (practice == null)
        {
            return NotFound("Practice not found");
        }

        var result = _mapper.Map<PracticeDTO>(practice);
        var topic = await _context.Topics.Where(t => t.Id == result.TopicId).FirstOrDefaultAsync();
        result.TopicName = topic?.Name;
        
        if (String.IsNullOrEmpty(result.LinkImage))
        {
            result.LinkImage = topic?.LinkImage ?? "";
        }

        var questions = await _context.Questions.Where(q => q.PracticeId == id).ToListAsync();
        if (questions != null && questions.Count > 0)
        {
            result.NumberOfQuestions = questions.Count;
            result.TimeDuration = questions.Count * 2;
        }
        else
        {
            result.NumberOfQuestions = 0;
            result.TimeDuration = 0;
        }
        var questionDtos = _mapper.Map<List<QuestionDTO>>(questions);
        result.Questions = questionDtos;
        return Ok(result);
    }
    // ----------------------------------------------------------------------
    // --- POST/PUT (Sửa để nhận [FromForm] và Image) ---
    // ----------------------------------------------------------------------
    [AdminCheck]
    [HttpPost]
    public async Task<ActionResult<PracticeDTO>> CreatePractice([FromForm] CreatePracticeDTO request) // Dùng [FromForm] và DTO mới
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var topicExists = await _context.Topics.AnyAsync(t => t.Id == request.TopicId);
        if (!topicExists)
        {
            return NotFound("Topic not found.");
        }

        string imagePathDb = "";

        // Xử lý lưu file nếu có ảnh gửi lên
        if (request.Image != null && request.Image.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
            var folderPath = Path.Combine(_env.WebRootPath, "images");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            imagePathDb = $"/images/{fileName}";
        }

        var newPractice = new Practice
        {
            Id = Guid.CreateVersion7(),
            TopicId = request.TopicId,
            Name = request.Name,
            Level = request.Level,
            LinkImage = imagePathDb, // Lưu đường dẫn ảnh
            CreatedAt = DateTime.UtcNow.AddHours(7) 
        };

        _context.Practices.Add(newPractice);
        await _context.SaveChangesAsync();
        
        var result = _mapper.Map<PracticeDTO>(newPractice);
        return Ok(result);
    }
    [AdminCheck]
    [HttpPut("{id}")]
    public async Task<ActionResult<PracticeDTO>> UpdatePractice(Guid id, [FromForm] CreatePracticeDTO request) // Dùng [FromForm] và DTO mới
    {
        var practice = await _context.Practices.FindAsync(id);
        if (practice == null)
            return NotFound("Practice not found");

        var topicExists = await _context.Topics.AnyAsync(t => t.Id == request.TopicId);
        if (!topicExists)
        {
            return NotFound("Topic not found.");
        }

        // Cập nhật các trường không liên quan đến ảnh
        practice.TopicId = request.TopicId;
        practice.Name = request.Name;
        practice.Level = request.Level;
        
        // Xử lý ảnh mới (nếu người dùng có chọn ảnh mới)
        if (request.Image != null && request.Image.Length > 0)
        {
            // 1. (Optional) Xóa ảnh cũ
            if (!string.IsNullOrEmpty(practice.LinkImage))
            {
                var oldPath = Path.Combine(_env.WebRootPath, practice.LinkImage.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            // 2. Lưu ảnh mới
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
            var folderPath = Path.Combine(_env.WebRootPath, "images");
            
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            practice.LinkImage = $"/images/{fileName}";
        }
        // Nếu request.Image == null, LinkImage trong DB vẫn được giữ nguyên

        _context.Practices.Update(practice);
        await _context.SaveChangesAsync();
        
        var result = _mapper.Map<PracticeDTO>(practice);
        return Ok(result);
    }
    
    // --- DELETE (Giữ nguyên logic cũ) ---
    [AdminCheck]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePractice(Guid id)
    {
        var practice = await _context.Practices.FindAsync(id);
        if (practice == null)
            return NotFound("Practice not found");

        // (Optional) Xóa ảnh liên kết
        if (!string.IsNullOrEmpty(practice.LinkImage))
        {
             var oldPath = Path.Combine(_env.WebRootPath, practice.LinkImage.TrimStart('/'));
             if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }
        
        _context.Practices.Remove(practice);
        await _context.SaveChangesAsync();
        return Ok("Deleted successfully");
    }

    // --- SUBMIT PRACTICE (Giữ nguyên logic cũ) ---

    // Chấm diem bai luyen tap
    [HttpPost("submit")]
    [SessionCheck]
    public async Task<ActionResult<SubmitPracticeDTO>> SubmitPractice([FromBody] SubmitPracticeDTO dto)
    {
        if (dto.submits == null || dto.submits.Count == 0)
        {
            dto.Score = 0m;
            dto.NumberOfCorrects = 0;
            dto.TotalQuestions = 0;
            return Ok(dto);
        }
        
        // Lấy userId từ header
        string headerUserId = Request.Headers["userId"].FirstOrDefault();

        if (string.IsNullOrEmpty(headerUserId) || !headerUserId.Equals(dto.UserId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = "User ID mismatch or missing in headers." });
        }
        
        // --- 1. Truy vấn đáp án đúng từ DB (Tối ưu hóa) ---
        var questionIds = dto.submits.Select(s => s.Id).ToList();
        var correctAnswersFromDb = await _context.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, q => q.CorrectOption);

        // --- 2. Chấm điểm và tính toán ---
        dto.NumberOfCorrects = 0;
        dto.TotalQuestions = correctAnswersFromDb.Count;

        foreach (var submit in dto.submits)
        {
            if (correctAnswersFromDb.TryGetValue(submit.Id, out string correctOption))
            {
                submit.CorrectAnswer = correctOption;

                if (!String.IsNullOrEmpty(submit.UserAnswer) &&
                    submit.UserAnswer.Equals(correctOption, StringComparison.OrdinalIgnoreCase))
                {
                    dto.NumberOfCorrects++;
                }
            }
        }

        // --- 3. Tính điểm (Sửa lỗi chia số nguyên) ---
        decimal maxScore = 10.0m;
        if (dto.TotalQuestions > 0)
        {
            dto.Score = Math.Round((decimal)dto.NumberOfCorrects.Value / dto.TotalQuestions * maxScore, 2);
        }
        else
        {
            dto.Score = 0m;
        }

        // --- 4. Lưu vào bảng Result ---
        var currentResult =
            await _context.Results.FirstOrDefaultAsync(x => x.TargetId == dto.PracticeId && x.UserId == dto.UserId
            );

        if (currentResult == null)
        {
            Result result = new Result()
            {
                UserId = dto.UserId,
                TargetId = dto.PracticeId,
                PracticeType = 1, // Multiple choice
                Score = dto.Score,
                TotalQuestions = dto.TotalQuestions,
                CorrectAnswers = dto.NumberOfCorrects,
                WritingText = "",
                WritingFeedback = "",
                ChatHistoryJson = "",
                ChatEvaluation = "",
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdatedAt = DateTime.UtcNow.AddHours(7)
            };
            _context.Results.Add(result);
        }
        else
        {
            currentResult.Score = dto.Score;
            currentResult.TotalQuestions = dto.TotalQuestions;
            currentResult.CorrectAnswers = dto.NumberOfCorrects;
            currentResult.UpdatedAt = DateTime.UtcNow.AddHours(7);
            _context.Results.Update(currentResult);
        }

        await _context.SaveChangesAsync();

        return Ok(dto);
    }
}
public class CreatePracticeDTO
{
    [Required] public Guid TopicId { get; set; }
    [Required, MaxLength(255)] public string Name { get; set; }
    public int Level { get; set; } = 1;
    
    // Đây là biến hứng file ảnh từ React (key='image')
    public IFormFile? Image { get; set; }
}