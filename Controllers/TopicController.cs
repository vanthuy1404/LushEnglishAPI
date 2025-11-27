// 19/11/2025 - 20:32:27
// DANGTHUY

using AutoMapper;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicController(LushEnglishDbContext context, IMapper mapper) : ControllerBase
{
    private readonly LushEnglishDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    // GET: api/Topic
    [HttpGet]
    public ActionResult<List<TopicDTO>> GetAllTopics()
    {
        var topics = _context.Topics.ToList();
        var results = _mapper.Map<List<TopicDTO>>(topics);
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

    // POST: api/Topic
    [HttpPost]
    public async Task<ActionResult<TopicDTO?>> CreateTopic([FromBody] TopicDTO topic)
    {
        var newTopic = new Topic()
        {
            Id = Guid.CreateVersion7(),
            Name = topic.Name,
            Description = topic.Description,
            YoutubeUrl = topic.YoutubeUrl,
            Level = topic.Level,
            LinkImage = topic.LinkImage,
            CreatedAt = DateTime.UtcNow.AddHours(7)
        };

        await _context.Topics.AddAsync(newTopic);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<TopicDTO>(newTopic);
        return Ok(result);
    }

    // PUT: api/Topic/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<TopicDTO?>> UpdateTopic(Guid id, [FromBody] TopicDTO topicDto)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
            return NotFound("Topic not found");

        topic.Name = topicDto.Name;
        topic.Description = topicDto.Description;
        topic.YoutubeUrl = topicDto.YoutubeUrl;
        topic.Level = topicDto.Level;
        topic.LinkImage = topicDto.LinkImage;
        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<TopicDTO>(topic);
        return Ok(result);
    }
}
