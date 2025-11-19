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

        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<TopicDTO>(topic);
        return Ok(result);
    }
}
