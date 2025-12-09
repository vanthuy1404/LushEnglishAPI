// 27/11/2025 - 10:01:49 (Updated for Image Upload)
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using AutoMapper;
using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WritingController(
    LushEnglishDbContext context,
    IMapper mapper,
    IOptions<GeminiApiSettings> apiSettings,
    IHttpClientFactory httpClientFactory,
    // 1. Thêm IWebHostEnvironment
    IWebHostEnvironment env) : ControllerBase
{
    // GET: api/Writing (Giữ nguyên)
    [HttpGet]
    [SessionCheck]
    public async Task<ActionResult<List<WritingExerciseDTO>>> GetWritingExercises()
    {
        var writingConfigs = await context.WritingConfigs.ToListAsync();
        var results = mapper.Map<List<WritingExerciseDTO>>(writingConfigs);
        foreach (var result in results)
        {
            var topic = await context.Topics.FindAsync(result.TopicId);
            if (topic != null)
            {
                result.TopicName = topic.Name;
                if (String.IsNullOrEmpty(result.LinkImage))
                {
                    result.LinkImage = topic.LinkImage;
                }
            }
        }
        return Ok(results);
    }

    // GET: api/Writing/{id} (Giữ nguyên)
    [HttpGet("{id}")]
    [SessionCheck]
    public async Task<ActionResult<WritingExerciseDTO>> GetWritingExercise(Guid id)
    {
        var writingConfig = await context.WritingConfigs.FindAsync(id);

        if (writingConfig == null)
        {
            return NotFound("Writing Exercise not found");
        }

        var result = mapper.Map<WritingExerciseDTO>(writingConfig);
        var topic = await context.Topics.FindAsync(result.TopicId);
        if (topic != null)
        {
            result.TopicName = topic.Name;
            if (String.IsNullOrEmpty(result.LinkImage))
            {
                result.LinkImage = topic.LinkImage;
            }
        }

        return Ok(result);
    }

    // ---------------------------------------------------------
    // POST: api/Writing (CẬP NHẬT: Dùng FromForm & Lưu ảnh)
    // ---------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<WritingExerciseDTO>> CreateWritingExercise([FromForm] CreateWritingExerciseDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Kiểm tra Topic tồn tại
        var topicExists = await context.Topics.AnyAsync(t => t.Id == dto.TopicId);
        if (!topicExists)
        {
            return NotFound("Topic not found.");
        }

        string imagePathDb = ""; // Đường dẫn lưu DB

        // Xử lý lưu file ảnh
        if (dto.Image != null && dto.Image.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var folderPath = Path.Combine(env.WebRootPath, "images");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            imagePathDb = $"/images/{fileName}";
        }

        var writingConfig = new WritingConfig
        {
            Id = Guid.NewGuid(),
            TopicId = dto.TopicId,
            Name = dto.Name,
            Requirement = dto.Requirement,
            Level = dto.Level,
            LinkImage = imagePathDb, // Lưu đường dẫn ảnh
            CreatedAt = DateTime.UtcNow
        };

        context.WritingConfigs.Add(writingConfig);
        await context.SaveChangesAsync();

        var result = mapper.Map<WritingExerciseDTO>(writingConfig);
        return CreatedAtAction(nameof(GetWritingExercise), new { id = writingConfig.Id }, result);
    }

    // ---------------------------------------------------------
    // PUT: api/Writing/{id} (CẬP NHẬT: Dùng FromForm & Lưu ảnh)
    // ---------------------------------------------------------
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateWritingExercise(Guid id, [FromForm] CreateWritingExerciseDTO dto)
    {
        var writingConfig = await context.WritingConfigs.FindAsync(id);

        if (writingConfig == null)
            return NotFound("Writing Exercise not found");

        // Cập nhật thông tin cơ bản
        writingConfig.TopicId = dto.TopicId;
        writingConfig.Name = dto.Name;
        writingConfig.Requirement = dto.Requirement;
        writingConfig.Level = dto.Level;

        // Xử lý ảnh mới (nếu có)
        if (dto.Image != null && dto.Image.Length > 0)
        {
            // 1. Xóa ảnh cũ (Optional)
            if (!string.IsNullOrEmpty(writingConfig.LinkImage))
            {
                var oldPath = Path.Combine(env.WebRootPath, writingConfig.LinkImage.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            // 2. Lưu ảnh mới
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var folderPath = Path.Combine(env.WebRootPath, "images");

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            writingConfig.LinkImage = $"/images/{fileName}";
        }
        // Nếu dto.Image == null, giữ nguyên LinkImage cũ

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

        return NoContent();
    }

    // DELETE: api/Writing/{id} (Giữ nguyên)
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWritingExercise(Guid id)
    {
        var writingConfig = await context.WritingConfigs.FindAsync(id);

        if (writingConfig == null)
            return NotFound("Writing Exercise not found");

        // (Optional) Xóa ảnh khi xóa bài tập
        if (!string.IsNullOrEmpty(writingConfig.LinkImage))
        {
            var oldPath = Path.Combine(env.WebRootPath, writingConfig.LinkImage.TrimStart('/'));
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }

        context.WritingConfigs.Remove(writingConfig);
        await context.SaveChangesAsync();

        return Ok("Writing Exercise deleted successfully");
    }

    // SUBMIT (Giữ nguyên toàn bộ logic cũ)
    [HttpPost("submit")]
    [SessionCheck]
    public async Task<ActionResult<WritingResultResponse>> SubmitWritingExercise([FromBody] SubmitWritingDTO dto)
    {
        // Kiểm tra dữ liệu đầu vào bắt buộc
        if (dto == null || string.IsNullOrWhiteSpace(dto.Requirements) ||
            string.IsNullOrWhiteSpace(dto.UserParagraphs) || dto.UserId == Guid.Empty ||
            dto.WritingExerciseId == Guid.Empty)
        {
            return BadRequest(
                "Invalid request data. Requirements, UserParagraphs, UserId, and WritingExerciseId must be provided.");
        }
        // Lấy userId từ header
        string headerUserId = Request.Headers["userId"].FirstOrDefault();

        if (string.IsNullOrEmpty(headerUserId) || !headerUserId.Equals(dto.UserId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = "User ID mismatch or missing in headers." });
        }
        
        // --- Prompt ---
        string prompt =
            "You are an **encouraging and experienced English language tutor** specializing in providing helpful, growth-oriented feedback. Your task is to evaluate a user's writing on a scale of 10 and provide detailed, supportive feedback. The scoring should be **flexible**, focused on identifying areas for improvement rather than strict academic grading. If the user meets the requirements 'about 150 words', slight deviations (e.g., 120-180 words) should **not** result in a score penalty.\n" +
            $"## Assignment Details and User's Submission:\n" +
            $"- **Requirements**: {dto.Requirements}\n" +
            $"- **User's Paragraphs**: \"{dto.UserParagraphs}\"\n\n" +
            $"## Evaluation Criteria and Output Format:\n" +
            $"1. **Score**: Provide a final score out of 10. The score should **reflect the current learning stage** of the user. Be generous if the key ideas are communicated clearly. Scoring consideration should be weighted as follows:\n" +
            $"   - **Grammar & Vocabulary (40%)**: Focus on clarity and major errors. Avoid penalizing for commonly used but technically less sophisticated words if they are correct.\n" +
            $"   - **Coherence & Flow (30%)**: Assess how easily the text can be understood.\n" +
            $"   - **Requirement Fulfillment (30%)**: Check if the topic is covered. **Be highly flexible with word count** since the requirement uses 'about'.\n" +
            $"2. **Overall Evaluation**: Write a **positive and encouraging** general comment. Highlight strengths first, then gently point out 1-2 main areas for future practice.\n" +
            $"3. **Correct Version**: Provide a fully corrected version. Use the improved version **only to fix errors and make necessary flow/coherence improvements**. Avoid drastic vocabulary changes if the original words are correct and appropriate.\n" +
            $"4. **Detail Evaluates**: Analyze the user's text and provide specific points of improvement as a list of JSON objects. For each point, use 'error' only for **mandatory grammar/syntax corrections**. Use 'suggestion' for **all** stylistic, vocabulary, or flow improvements, making it clear these are optional steps to advanced English.\n\n" +
            $"Return the complete result *only* in the following strict JSON format. Ensure all fields are included and properly formatted:\n\n" +
            $@"{{
      ""Score"": <integer_score_out_of_10>,
      ""OverallEvaluate"": ""<string_general_feedback>"",
      ""CorrectVersion"": ""<string_corrected_paragraph>"",
      ""DetailEvaluates"": [
        {{
          ""Type"": ""error"" | ""suggestion"",
          ""Original"": ""<string_original_text_segment_with_mistake>"",
          ""Corrected"": ""<string_corrected_text_segment>"",
          ""Explaination"": ""<string_explanation_of_the_change>""
        }},
      ]
    }}";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };
        string fullApiUrl = $"{apiSettings.Value.ApiUrl}?key={apiSettings.Value.ApiKey}";

        // GỌI GEMINI API
        using (HttpClient client = httpClientFactory.CreateClient())
        {
            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(fullApiUrl, content);
            string jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, $"API error: {jsonResponse}");
            }

            try
            {
                // XỬ LÝ PHẢN HỒI GEMINI
                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    var root = doc.RootElement;
                    var jsonTextElement = root
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text");

                    string jsonText = jsonTextElement.GetString();

                    if (string.IsNullOrWhiteSpace(jsonText))
                    {
                        return BadRequest("Invalid AI response format: Empty text field.");
                    }

                    // Xử lý loại bỏ markdown JSON block
                    jsonText = jsonText.Replace("```json", "").Replace("```", "").Trim();

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var resultResponse = JsonSerializer.Deserialize<WritingResultResponse>(jsonText, options);

                    if (resultResponse == null)
                    {
                        return BadRequest("Failed to deserialize AI response into WritingResultResponse.");
                    }

                    // --- LOGIC LƯU HOẶC CẬP NHẬT KẾT QUẢ VÀO DB ---
                    var existingResult = await context.Results
                        .FirstOrDefaultAsync(r =>
                            r.UserId == dto.UserId &&
                            r.TargetId == dto.WritingExerciseId &&
                            r.PracticeType == 2); 

                    string writingFeedbackJson = JsonSerializer.Serialize(resultResponse, options);

                    if (existingResult != null)
                    {
                        existingResult.Score = resultResponse.Score;
                        existingResult.WritingText = dto.UserParagraphs; 
                        existingResult.WritingFeedback = writingFeedbackJson; 
                        existingResult.UpdatedAt = DateTime.Now;

                        context.Results.Update(existingResult);
                    }
                    else
                    {
                        var newResult = new Result
                        {
                            UserId = dto.UserId,
                            TargetId = dto.WritingExerciseId, 
                            PracticeType = 2, 

                            Score = resultResponse.Score,
                            ChatEvaluation = "",
                            ChatHistoryJson = "",
                            WritingText = dto.UserParagraphs,
                            WritingFeedback = writingFeedbackJson,

                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        context.Results.Add(newResult);
                    }

                    await context.SaveChangesAsync();
                    return Ok(resultResponse);
                }
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Error parsing JSON: {ex.Message}. Raw response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
public class CreateWritingExerciseDTO
{
    [Required]
    public Guid TopicId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Requirement { get; set; } // Chuỗi JSON hoặc text yêu cầu

    [Required]
    public int Level { get; set; } = 1;

    // Nhận file ảnh từ React
    public IFormFile? Image { get; set; }
}