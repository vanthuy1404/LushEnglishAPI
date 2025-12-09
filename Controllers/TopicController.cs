// 19/11/2025 - 20:32:27
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicController(LushEnglishDbContext context, IMapper mapper, IWebHostEnvironment env) : ControllerBase
{
    private readonly LushEnglishDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    // GET: api/Topic
    [HttpGet]
    public ActionResult<List<TopicDTO>> GetAllTopics()
    {
        var topics = _context.Topics.ToList();
        var results = _mapper.Map<List<TopicDTO>>(topics).OrderByDescending(x => x.CreatedAt).ToList();
        foreach (var result in results)
        {
            // Lấy từ vựng theo chủ đề
            var vocabs = _context.Vocabularies.Where(x => x.TopicId == result.Id).ToList();
            var vocabsDto = _mapper.Map<List<VocabularyDTO>>(vocabs);
            result.Vocabularies = vocabsDto;
            // Lay bai luyen tap
            var practices  = _context.Practices.Where(x => x.TopicId == result.Id).ToList();
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

    // GET: api/Topic/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TopicDTO?>> GetTopic(Guid id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
            return NotFound("Topic not found");

        var result = _mapper.Map<TopicDTO>(topic);
        
        // Lấy từ vựng theo chủ đề
        var vocabs = _context.Vocabularies.Where(x => x.TopicId == result.Id).ToList();
        var vocabsDto = _mapper.Map<List<VocabularyDTO>>(vocabs);
        result.Vocabularies = vocabsDto;
        // Lay bai luyen tap
        var practices  = _context.Practices.Where(x => x.TopicId == result.Id).ToList();
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
    public async Task<ActionResult<TopicDTO>> CreateTopic([FromForm] CreateTopicDTO request) // 3. Dùng [FromForm] và DTO mới
    {
        string imagePathDb = ""; // Đường dẫn để lưu vào DB

        // 4. Xử lý lưu file nếu có ảnh gửi lên
        if (request.Image != null && request.Image.Length > 0)
        {
            // Tạo tên file duy nhất: GUID + đuôi file gốc (vd: .jpg)
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
            
            // Đường dẫn vật lý: wwwroot/images
            var folderPath = Path.Combine(env.WebRootPath, "images");

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Đường dẫn file đầy đủ
            var filePath = Path.Combine(folderPath, fileName);

            // Lưu file xuống đĩa
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            // Đường dẫn tương đối lưu DB (để frontend hiển thị)
            imagePathDb = $"/images/{fileName}";
        }

        // 5. Map dữ liệu sang Entity
        var newTopic = new Topic()
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            Description = request.Description,
            YoutubeUrl = request.YoutubeUrl,
            Level = request.Level,
            LinkImage = imagePathDb, // Lưu đường dẫn /images/...
            CreatedAt = DateTime.UtcNow.AddHours(7)
        };

        await _context.Topics.AddAsync(newTopic);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<TopicDTO>(newTopic);
        return Ok(result);
    }

    /// PUT: api/Topic/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<TopicDTO>> UpdateTopic(Guid id, [FromForm] CreateTopicDTO request)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic == null) return NotFound("Topic not found");

        topic.Name = request.Name;
        topic.Description = request.Description;
        topic.YoutubeUrl = request.YoutubeUrl;
        topic.Level = request.Level;

        // Xử lý ảnh mới (nếu người dùng có chọn ảnh mới)
        if (request.Image != null && request.Image.Length > 0)
        {
            // (Optional) Xóa ảnh cũ đi cho đỡ rác server
            if (!string.IsNullOrEmpty(topic.LinkImage))
            {
                var oldPath = Path.Combine(env.WebRootPath, topic.LinkImage.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            // Lưu ảnh mới (Logic giống hệt Create)
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
        // Nếu request.Image == null thì giữ nguyên LinkImage cũ trong DB

        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<TopicDTO>(topic);
        return Ok(result);
    }
    // DELETE: api/Topic/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTopic(Guid id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
            return NotFound("Topic not found");

        // Xóa từ vựng
        var vocabs = _context.Vocabularies.Where(x => x.TopicId == id).ToList();
        _context.Vocabularies.RemoveRange(vocabs);

        // Xóa bài luyện tập
        var practices = _context.Practices.Where(x => x.TopicId == id).ToList();
        _context.Practices.RemoveRange(practices);

        // Xóa bài chatting
        var chattings = _context.ChattingConfigs.Where(x => x.TopicId == id).ToList();
        _context.ChattingConfigs.RemoveRange(chattings);

        // Xóa bài writing
        var writings = _context.WritingConfigs.Where(x => x.TopicId == id).ToList();
        _context.WritingConfigs.RemoveRange(writings);

        // Xóa topic
        _context.Topics.Remove(topic);

        await _context.SaveChangesAsync();

        return Ok("Delete topic successfully");
    }
}

public class CreateTopicDTO
{
    [Required] 
    public string Name { get; set; }
    public string Description { get; set; }
    public string YoutubeUrl { get; set; }
    public int Level { get; set; } = 1;
    
    // Đây là biến hứng file ảnh từ React (key='image')
    public IFormFile? Image { get; set; }
}
