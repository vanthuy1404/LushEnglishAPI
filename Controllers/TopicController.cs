// 19/11/2025 - updated
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
public class TopicController(LushEnglishDbContext context, IMapper mapper, IWebHostEnvironment env, TopicIdsService _topicIdsService ) : ControllerBase
{
    private readonly LushEnglishDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    // GET: api/Topic
    [HttpGet]
    public ActionResult<List<TopicDTO>> GetAllTopics()
    {
        var topics = _context.Topics.ToList();
        var results = _mapper.Map<List<TopicDTO>>(topics)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        // ===== NEW: Fill CourseName in bulk (avoid N+1) =====
        var courseIds = results
            .Where(x => x.CourseId.HasValue)
            .Select(x => x.CourseId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> courseNameMap = new();
        if (courseIds.Count > 0)
        {
            courseNameMap = _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name })
                .ToDictionary(x => x.Id, x => x.Name);
        }

        foreach (var result in results)
        {
            // set CourseName if any
            if (result.CourseId.HasValue && courseNameMap.TryGetValue(result.CourseId.Value, out var cName))
                result.CourseName = cName;

            // Lấy từ vựng theo chủ đề
            var vocabs = _context.Vocabularies.Where(x => x.TopicId == result.Id).ToList();
            var vocabsDto = _mapper.Map<List<VocabularyDTO>>(vocabs);
            result.Vocabularies = vocabsDto;

            // Lay bai luyen tap
            var practices = _context.Practices.Where(x => x.TopicId == result.Id).ToList();
            var practicesDto = _mapper.Map<List<PracticeDTO>>(practices);
            result.Practices = practicesDto;

            // Lay bai chatting
            var chattings = _context.ChattingConfigs.Where(x => x.TopicId == result.Id).ToList();
            var chattingDtos = _mapper.Map<List<ChattingExerciseDTO>>(chattings);
            result.ChattingExercises = chattingDtos;

            // lay bai writing
            var writings = _context.WritingConfigs.Where(x => x.TopicId == result.Id).ToList();
            var writingDtos = _mapper.Map<List<WritingExerciseDTO>>(writings);
            result.WritingExercises = writingDtos;
        }

        return Ok(results);
    }
    [HttpGet("my-topics")] public async Task<ActionResult<List<TopicDTO>>> GetMyTopics()
{
    List<Guid> topicIds;

    try
    {
        // lấy topicIds theo quyền học của user
        topicIds = await _topicIdsService.GetMyTopicIdsAsync();
    }
    catch (UnauthorizedAccessException)
    {
        // ❌ guest => KHÔNG trả gì
        return Ok(new List<TopicDTO>());
    }

    // user hợp lệ nhưng chưa mua course nào => KHÔNG trả gì
    if (topicIds.Count == 0)
        return Ok(new List<TopicDTO>());

    var topics = await _context.Topics
        .Where(t => topicIds.Contains(t.Id))
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();

    var results = _mapper.Map<List<TopicDTO>>(topics);

    // ===== Fill CourseName in bulk =====
    var courseIds = results
        .Where(x => x.CourseId.HasValue)
        .Select(x => x.CourseId!.Value)
        .Distinct()
        .ToList();

    Dictionary<Guid, string> courseNameMap = new();
    if (courseIds.Count > 0)
    {
        courseNameMap = await _context.Courses
            .Where(c => courseIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name);
    }

    foreach (var result in results)
    {
        if (result.CourseId.HasValue &&
            courseNameMap.TryGetValue(result.CourseId.Value, out var cName))
        {
            result.CourseName = cName;
        }

        // fill exercises giống GetAllTopics
        result.Vocabularies = _mapper.Map<List<VocabularyDTO>>(
            await _context.Vocabularies.Where(x => x.TopicId == result.Id).ToListAsync()
        );

        result.Practices = _mapper.Map<List<PracticeDTO>>(
            await _context.Practices.Where(x => x.TopicId == result.Id).ToListAsync()
        );

        result.ChattingExercises = _mapper.Map<List<ChattingExerciseDTO>>(
            await _context.ChattingConfigs.Where(x => x.TopicId == result.Id).ToListAsync()
        );

        result.WritingExercises = _mapper.Map<List<WritingExerciseDTO>>(
            await _context.WritingConfigs.Where(x => x.TopicId == result.Id).ToListAsync()
        );
    }

    return Ok(results);
}


    // GET: api/Topic/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TopicDTO?>> GetTopic(Guid id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
            return NotFound("Topic not found");

        var result = _mapper.Map<TopicDTO>(topic);

        // ===== NEW: Fill CourseName if any =====
        if (result.CourseId.HasValue)
        {
            result.CourseName = await _context.Courses
                .Where(c => c.Id == result.CourseId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }

        // Lấy từ vựng theo chủ đề
        var vocabs = _context.Vocabularies.Where(x => x.TopicId == result.Id).ToList();
        var vocabsDto = _mapper.Map<List<VocabularyDTO>>(vocabs);
        result.Vocabularies = vocabsDto;

        // Lay bai luyen tap
        var practices = _context.Practices.Where(x => x.TopicId == result.Id).ToList();
        var practicesDto = _mapper.Map<List<PracticeDTO>>(practices);
        result.Practices = practicesDto;

        // Lay bai chatting
        var chattings = _context.ChattingConfigs.Where(x => x.TopicId == result.Id).ToList();
        var chattingDtos = _mapper.Map<List<ChattingExerciseDTO>>(chattings);
        result.ChattingExercises = chattingDtos;

        // lay bai writing
        var writings = _context.WritingConfigs.Where(x => x.TopicId == result.Id).ToList();
        var writingDtos = _mapper.Map<List<WritingExerciseDTO>>(writings);
        result.WritingExercises = writingDtos;

        return Ok(result);
    }

    [HttpPost]
    [AdminCheck]
    public async Task<ActionResult<TopicDTO>> CreateTopic([FromForm] CreateTopicDTO request)
    {
        string imagePathDb = "";

        if (request.Image != null && request.Image.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
            var folderPath = Path.Combine(env.WebRootPath, "images");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            imagePathDb = $"/images/{fileName}";
        }

        var newTopic = new Topic()
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            Description = request.Description,
            YoutubeUrl = request.YoutubeUrl,
            Level = request.Level,
            LinkImage = imagePathDb,
            CreatedAt = DateTime.UtcNow.AddHours(7),
            // CourseId left null (loose)
        };

        await _context.Topics.AddAsync(newTopic);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<TopicDTO>(newTopic);
        // CourseName stays null
        return Ok(result);
    }

    /// PUT: api/Topic/{id}
    [HttpPut("{id}")]
    [AdminCheck]
    public async Task<ActionResult<TopicDTO>> UpdateTopic(Guid id, [FromForm] CreateTopicDTO request)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic == null) return NotFound("Topic not found");

        topic.Name = request.Name;
        topic.Description = request.Description;
        topic.YoutubeUrl = request.YoutubeUrl;
        topic.Level = request.Level;

        if (request.Image != null && request.Image.Length > 0)
        {
            if (!string.IsNullOrEmpty(topic.LinkImage))
            {
                var oldPath = Path.Combine(env.WebRootPath, topic.LinkImage.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
            var folderPath = Path.Combine(env.WebRootPath, "images");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            topic.LinkImage = $"/images/{fileName}";
        }

        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<TopicDTO>(topic);

        // Fill CourseName if any
        if (result.CourseId.HasValue)
        {
            result.CourseName = await _context.Courses
                .Where(c => c.Id == result.CourseId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }

        return Ok(result);
    }

    // DELETE: api/Topic/{id}
    [HttpDelete("{id}")]
    [AdminCheck]
    public async Task<ActionResult> DeleteTopic(Guid id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
            return NotFound("Topic not found");

        var vocabs = _context.Vocabularies.Where(x => x.TopicId == id).ToList();
        _context.Vocabularies.RemoveRange(vocabs);

        var practices = _context.Practices.Where(x => x.TopicId == id).ToList();
        _context.Practices.RemoveRange(practices);

        var chattings = _context.ChattingConfigs.Where(x => x.TopicId == id).ToList();
        _context.ChattingConfigs.RemoveRange(chattings);

        var writings = _context.WritingConfigs.Where(x => x.TopicId == id).ToList();
        _context.WritingConfigs.RemoveRange(writings);

        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();

        return Ok("Delete topic successfully");
    }

    // ===== NEW API: api/Topic/addToCourse =====
    // Input: courseId + listTopicId
    // Logic: assign CourseId to those topics
    [HttpPost("addToCourse")]
    [AdminCheck]
    public async Task<ActionResult> AddToCourse([FromBody] AddToCourseRequestDTO request)
    {
        if (request.TopicIds == null || request.TopicIds.Count == 0)
            return BadRequest("TopicIds is required");

        // Check course exists
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == request.CourseId);
        if (!courseExists)
            return NotFound("Course not found");

        // Load topics by ids
        var topics = await _context.Topics
            .Where(t => request.TopicIds.Contains(t.Id))
            .ToListAsync();

        if (topics.Count == 0)
            return NotFound("No topics found");

        // Assign
        foreach (var t in topics)
            t.CourseId = request.CourseId;

        await _context.SaveChangesAsync();

        // Return updated topics (with CourseName)
        var results = _mapper.Map<List<TopicDTO>>(topics);
        var courseName = await _context.Courses
            .Where(c => c.Id == request.CourseId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();

        foreach (var r in results)
            r.CourseName = courseName;

        return Ok(results);
    }
}

public class CreateTopicDTO
{
    [Required]
    public string Name { get; set; }

    public string? Description { get; set; }
    public string? YoutubeUrl { get; set; }
    public int Level { get; set; } = 1;

    // Đây là biến hứng file ảnh từ React (key='image')
    public IFormFile? Image { get; set; }
}

public class AddToCourseRequestDTO
{
    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public List<Guid> TopicIds { get; set; } = new();
}
