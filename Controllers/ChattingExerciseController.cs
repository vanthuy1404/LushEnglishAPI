// 27/11/2025 - 22:00:00
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using AutoMapper;
using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;
using LushEnglishAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChattingController(
    LushEnglishDbContext context,
    IMapper mapper,
    IHttpClientFactory httpClientFactory,
    IOptions<GeminiApiSettings> apiSettings,
    IWebHostEnvironment env,
    TopicIdsService _topicIdsService) : ControllerBase
{
    // GET: api/Chatting
    [HttpGet]
    [SessionCheck]
    public async Task<ActionResult<List<ChattingExerciseDTO>>> GetChattingConfigs()
    {
        var chattingConfigs = await context.ChattingConfigs.ToListAsync();

        // Sử dụng AutoMapper để map từ Entity sang DTO
        var results = mapper.Map<List<ChattingExerciseDTO>>(chattingConfigs);
        foreach (var result in results)
        {
            var topic = await context.Topics.FindAsync(result.TopicId);
            if (topic != null)
            {
                result.TopicName = topic.Name;
                result.LinkImage = topic.LinkImage;
            }
        }

        return Ok(results);
    }
    // GET: api/Chatting/my-chattings
    [HttpGet("my-chattings")]
    public async Task<ActionResult<List<ChattingExerciseDTO>>> GetMyChattings()
    {
        List<Guid> topicIds;

        try
        {
            topicIds = await _topicIdsService.GetMyTopicIdsAsync();
        }
        catch (UnauthorizedAccessException)
        {
            // guest => không trả gì
            return Ok(new List<ChattingExerciseDTO>());
        }

        if (topicIds.Count == 0)
            return Ok(new List<ChattingExerciseDTO>());

        // Lấy chatting theo quyền topicIds
        var chattingConfigs = await context.ChattingConfigs
            .Where(c => topicIds.Contains(c.TopicId))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var results = mapper.Map<List<ChattingExerciseDTO>>(chattingConfigs);

        // ===== Bulk load Topic để fill TopicName + fallback LinkImage (tránh N+1) =====
        var topicIdList = results.Select(x => x.TopicId).Distinct().ToList();

        var topicMap = await context.Topics
            .Where(t => topicIdList.Contains(t.Id))
            .Select(t => new { t.Id, t.Name, t.LinkImage })
            .ToDictionaryAsync(x => x.Id, x => x);

        foreach (var r in results)
        {
            if (topicMap.TryGetValue(r.TopicId, out var t))
            {
                r.TopicName = t.Name;

                // Nếu ChattingConfig có LinkImage riêng thì giữ,
                // còn nếu rỗng thì fallback lấy ảnh Topic
                if (string.IsNullOrEmpty(r.LinkImage))
                    r.LinkImage = t.LinkImage ?? "";
            }
        }

        return Ok(results);
    }

    // GET: api/Chatting/5a70d10c-f37c-4a3e-b873-1f1966a3311e
    [HttpGet("{id}")]
    [SessionCheck]
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
    [AdminCheck]
    [HttpPost]
    public async Task<ActionResult<ChattingExerciseDTO>> CreateChattingConfig([FromForm] CreateChattingExerciseDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Kiểm tra Topic tồn tại
        var topicExists = await context.Topics.AnyAsync(t => t.Id == dto.TopicId);
        if (!topicExists)
        {
            return NotFound("Topic not found.");
        }

        string imagePathDb = ""; // Đường dẫn lưu DB

        // 2. Xử lý lưu file ảnh (nếu có)
        if (dto.Image != null && dto.Image.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var folderPath = Path.Combine(env.WebRootPath, "images"); // Cần IWebHostEnvironment env trong constructor

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

        // 3. Map DTO sang Entity
        var chattingConfig = new ChattingConfig
        {
            Id = Guid.NewGuid(),
            TopicId = dto.TopicId,
            Name = dto.Name,
            AiRole = dto.AiRole,
            Requirement = dto.Requirement,
            Objective = dto.Objective,
            MaxAiReplies = dto.MaxAiReplies,
            OpeningQuestion = dto.OpeningQuestion,
            Level = dto.Level,
            LinkImage = imagePathDb, // Lưu đường dẫn ảnh
            CreatedAt = DateTime.UtcNow
        };

        context.ChattingConfigs.Add(chattingConfig);
        await context.SaveChangesAsync();

        // 4. Map Entity đã lưu trở lại DTO để trả về
        var result = mapper.Map<ChattingExerciseDTO>(chattingConfig);
        return CreatedAtAction(nameof(GetChattingConfig), new { id = chattingConfig.Id }, result);
    }

    // PUT: api/Chatting/5a70d10c-f37c-4a3e-b873-1f1966a3311e
    [AdminCheck]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateChattingConfig(Guid id, [FromForm] CreateChattingExerciseDTO dto)
    {
        var chattingConfig = await context.ChattingConfigs.FindAsync(id);

        if (chattingConfig == null)
            return NotFound("Chatting Configuration not found");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Cập nhật thông tin cơ bản
        chattingConfig.TopicId = dto.TopicId;
        chattingConfig.Name = dto.Name;
        chattingConfig.AiRole = dto.AiRole;
        chattingConfig.Requirement = dto.Requirement;
        chattingConfig.Objective = dto.Objective;
        chattingConfig.MaxAiReplies = dto.MaxAiReplies;
        chattingConfig.OpeningQuestion = dto.OpeningQuestion;
        chattingConfig.Level = dto.Level;

        // 2. Xử lý ảnh mới (nếu có)
        if (dto.Image != null && dto.Image.Length > 0)
        {
            // a. Xóa ảnh cũ (Optional)
            if (!string.IsNullOrEmpty(chattingConfig.LinkImage))
            {
                var oldPath = Path.Combine(env.WebRootPath, chattingConfig.LinkImage.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            // b. Lưu ảnh mới
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var folderPath = Path.Combine(env.WebRootPath, "images");

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            chattingConfig.LinkImage = $"/images/{fileName}";
        }
        // Nếu dto.Image == null, giữ nguyên LinkImage cũ

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

        return NoContent();
    }

    // DELETE: api/Chatting/5a70d10c-f37c-4a3e-b873-1f1966a3311e
    [AdminCheck]
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

    //POST: api/Chatting/ai-response
    [HttpPost("ai-response")]
    [SessionCheck]
    public async Task<IActionResult> GetAiResponseMessage([FromBody] SubmitChattingDTO? dto)
    {
        // 1. Kiểm tra dữ liệu đầu vào bắt buộc
        if (dto == null || dto.ChatMessages == null || !dto.ChatMessages.Any() ||
            string.IsNullOrWhiteSpace(dto.Requirements) || string.IsNullOrWhiteSpace(dto.Objective) ||
            dto.ChattingExerciseId == Guid.Empty)
        {
            return BadRequest("Invalid request data. Missing required fields.");
        }

// Đếm số lần AI đã phản hồi
        int aiReplyCount = dto.ChatMessages.Count(m => m.Role.ToLower() == "assistant");

        // Tính số lượt còn lại
        int remainingReplies = dto.MaxAiReplies - aiReplyCount;

        // 2. Kiểm tra giới hạn lượt phản hồi và kết thúc cuộc trò chuyện
        if (remainingReplies <= 0)
        {
            return Ok(new AIResponseMessage { Content = "LESSON_COMPLETE" });
        }

        // 3. Xây dựng Prompt đơn nhất (Single Prompt Construction)
        string chatObjective = dto.Objective.Replace("**", "");

        // --- LOGIC MỚI: XÁC ĐỊNH CHỈ DẪN CUỐI CÙNG (DYNAMIC INSTRUCTION) ---
        string closingInstruction;

        if (remainingReplies == 1)
        {
            // TRƯỜNG HỢP: Lượt trả lời CUỐI CÙNG
            closingInstruction =
                "- **FINAL TURN RULE**: This is your LAST message. The user cannot reply after this.\n" +
                "- Do **NOT** ask any questions.\n" +
                "- Do **NOT** use phrases like 'What about you?' or 'Do you agree?'.\n" +
                "- Bring the conversation to a polite and natural end (e.g., 'It was great chatting with you', 'Good luck with your practice').";
        }
        else
        {
            // TRƯỜNG HỢP: Vẫn còn lượt trò chuyện
            closingInstruction =
                "- Ensure your response is complete, polite, and **encourages the user to reply**.\n" +
                "- You may ask a relevant follow-up question to keep the flow going.";
        }
        // -------------------------------------------------------------------

        // Phần I: System Instruction (Ngữ cảnh và Quy tắc)
        string prompt =
            // Đặt vai trò và mục tiêu
            $"You are an English language conversational AI. Your role is: '{dto.Requirements}'. " +
            $"The chat objective is: '{chatObjective}'. " +
            $"Strictly maintain your persona and stick to the given topic. " +
            $"Your response must be natural, engaging, and in English.\n\n" +
            $"**RESPONSE GUIDELINES**:\n" +
            $"- Keep your response **concise and conversational**, similar to a real-time chat message.\n" +
            $"- Avoid long paragraphs or lecturing. Aim for **2 to 4 sentences**.\n" +

            // --- CHÈN CHỈ DẪN ĐỘNG VÀO ĐÂY ---
            $"{closingInstruction}\n\n" +
            // ----------------------------------

            // Quy tắc kết thúc sớm (nếu người dùng muốn dừng)
            $"**EARLY FINISH RULE**: Even if you have turns left, if the user explicitly says goodbye or indicates they are done, you MUST return the exact phrase 'LESSON_COMPLETE' as your entire response content.\n\n" +

            // Phần II: Lịch sử cuộc trò chuyện
            "## Current Conversation History:\n";

        // Thêm lịch sử chat vào prompt
        foreach (var message in dto.ChatMessages)
        {
            prompt += $"- **{message.Role.ToUpper()}**: {message.Content}\n";
        }

        // Phần III: Yêu cầu hành động (Action Required)
        prompt += "\n## Action Required:\n" +
                  "Based on the history above, generate ONLY the next response from the AI assistant. Do not repeat history.";
        // 4. Xây dựng Request Body giống hệt cấu trúc Writing Exercise
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        // 5. GỌI GEMINI API
        string fullApiUrl = $"{apiSettings.Value.ApiUrl}?key={apiSettings.Value.ApiKey}";

        using (HttpClient client = httpClientFactory.CreateClient())
        {
            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(fullApiUrl, content);
            string jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Trả về lỗi API (bao gồm lỗi 400 nếu cấu trúc API bị sai)
                return StatusCode((int)response.StatusCode, $"API error: {jsonResponse}");
            }

            try
            {
                // 6. XỬ LÝ PHẢN HỒI GEMINI (giống Writing Exercise)
                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    var root = doc.RootElement;
                    var textElement = root
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text");

                    string aiContent = textElement.GetString();
                    if (String.IsNullOrEmpty(aiContent))
                    {
                        return StatusCode((int)response.StatusCode, "No AI content found");
                    }

                    if (string.IsNullOrWhiteSpace(aiContent))
                    {
                        return Ok(new AIResponseMessage { Content = "LESSON_COMPLETE" });
                    }

                    // Kiểm tra nếu AI tự quyết định kết thúc cuộc trò chuyện
                    if (aiContent.Equals("LESSON_COMPLETE", StringComparison.OrdinalIgnoreCase))
                    {
                        return Ok(new AIResponseMessage { Content = "LESSON_COMPLETE" });
                    }

                    // 7. Trả về Content: "xxxx"
                    var result = new AIResponseMessage
                    {
                        Content = aiContent
                    };

                    return Ok(result);
                }
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Error parsing AI response JSON: {ex.Message}. Raw response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    [HttpPost("evaluate")]
    [SessionCheck]
    public async Task<IActionResult> EvaluateChattingSession([FromBody] SubmitChattingDTO dto)
    {
        // 1. Kiểm tra dữ liệu đầu vào
        if (dto == null || dto.ChatMessages == null || !dto.ChatMessages.Any())
        {
            return BadRequest("No conversation data to evaluate.");
        }
        // Lấy userId từ header (Key phải khớp với cái bạn set trong axiosService: 'userId')
        string headerUserId = Request.Headers["userId"].FirstOrDefault();
        // [QUAN TRỌNG] Kiểm tra UserId vì cần lưu kết quả
        if (dto.UserId == null || dto.UserId == Guid.Empty)
        {
            return BadRequest("UserId is required to save the evaluation result.");
        }
        // Kiểm tra xem header có tồn tại không và có khớp với userId trong body gửi lên không
        // Sử dụng Equals để so sánh chuỗi an toàn
        if (string.IsNullOrEmpty(headerUserId) || !headerUserId.Equals(dto.UserId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            // Trả về 401 Unauthorized nếu không khớp
            return Unauthorized(new { message = "User ID mismatch or missing in headers." });
        }
        

        // 2. Xây dựng lịch sử hội thoại dạng Text cho AI đọc
        StringBuilder conversationText = new StringBuilder();
        foreach (var msg in dto.ChatMessages)
        {
            string role = msg.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ? "Student" : "AI Tutor";
            conversationText.AppendLine($"- {role}: {msg.Content}");
        }

        // 3. Xây dựng Prompt chấm điểm (Giữ nguyên logic cũ)
        string prompt =
            $"You are a **supportive and encouraging** English teacher evaluating a conversation practice session.\n" + // Sửa: supportive teacher thay vì expert teacher
            $"**Context/Scenario**: {dto.Requirements}\n" +
            $"**Objective**: {dto.Objective}\n\n" +
            $"**Conversation Transcript**:\n" +
            $"{conversationText}\n\n" +
            $"**GRADING CRITERIA (IMPORTANT)**:\n" + // Thêm phần quy tắc chấm điểm
            $"- **Be lenient**: Do NOT penalize for minor mechanical errors like **capitalization** (e.g., 'i' instead of 'I'), **punctuation**, or simple typos.\n" +
            $"- **Focus on Communication**: Prioritize whether the user successfully conveyed their meaning and achieved the objective over strict grammatical perfection.\n" +
            $"- **Naturalness**: If the user sounds natural but makes small grammar mistakes, still give a high score.\n\n" +
            $"**TASK**:\n" +
            $"Analyze the 'Student's' performance based on the criteria above. Ignore the AI Tutor's mistakes.\n" +
            $"**OUTPUT FORMAT (STRICT JSON)**:\n" +
            $"You must return ONLY a JSON object (no markdown, no extra text) with the following structure in **ENGLISH**:\n" +
            $"1. 'Score': An integer from **0 to 10** representing the overall quality (Be generous).\n" +
            $"2. 'OverallEvaluate': An encouraging general comment on the student's performance (in English).\n" +
            $"3. 'DetailEvaluates': An array of strings listing specific good points or significant errors (ignore minor ones) (in English).\n" +
            $"4. 'Suggestions': An array of strings giving advice for improvement (in English).\n\n" +
            $"Example JSON Structure:\n" +
            $"{{\n" +
            $"  \"Score\": 9,\n" +
            $"  \"OverallEvaluate\": \"You did a great job! Your communication was clear and natural despite some small typos.\",\n" +
            $"  \"DetailEvaluates\": [\"Good vocabulary choice\", \"Understood the context well\"],\n" +
            $"  \"Suggestions\": [\"Try to use more complex sentence structures next time\"]\n" +
            $"}}";
        // 4. Cấu hình Request Body cho Gemini
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        // 5. Gọi API Gemini
        string fullApiUrl = $"{apiSettings.Value.ApiUrl}?key={apiSettings.Value.ApiKey}";

        using (HttpClient client = httpClientFactory.CreateClient())
        {
            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(fullApiUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, $"API error: {jsonResponse}");
                }

                // 6. Xử lý kết quả trả về từ Gemini
                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        string? aiContent = candidates[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        // Làm sạch JSON
                        aiContent = CleanJsonString(aiContent);

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        // Deserialize vào DTO ChattingResult
                        // Lưu ý: Đảm bảo class ChattingResultResponse khớp với JSON trả về
                        ChattingResultResponse? resultData =
                            JsonSerializer.Deserialize<ChattingResultResponse>(aiContent, options);

                        if (resultData != null)
                        {
                            // =================================================================================
                            // 7. LƯU VÀO DATABASE (UPSERT LOGIC)
                            // =================================================================================

                            // Serialize lịch sử chat để lưu vào DB
                            string chatHistoryJson = JsonSerializer.Serialize(dto.ChatMessages);

                            // Serialize kết quả đánh giá để lưu vào DB (Dùng lại string aiContent hoặc serialize lại resultData)
                            string evaluationJson = JsonSerializer.Serialize(resultData);

                            // Tìm bản ghi cũ
                            var existingResult = await context.Results
                                .FirstOrDefaultAsync(r => r.UserId == dto.UserId
                                                          && r.TargetId == dto.ChattingExerciseId
                                                          && r.PracticeType == 3); // 3 = Chatting

                            if (existingResult != null)
                            {
                                // --- UPDATE ---
                                existingResult.Score = (decimal)resultData.Score;
                                existingResult.ChatHistoryJson = chatHistoryJson;
                                existingResult.ChatEvaluation = evaluationJson;
                                existingResult.UpdatedAt = DateTime.Now;

                                // _context.Results.Update(existingResult); // Không bắt buộc nếu tracking đang bật
                            }
                            else
                            {
                                // --- CREATE ---
                                var newResult = new Result
                                {
                                    UserId = dto.UserId.Value,
                                    TargetId = dto.ChattingExerciseId,
                                    PracticeType = 3, // Chatting
                                    Score = (decimal)resultData.Score,
                                    WritingFeedback = "",
                                    WritingText = "",
                                    ChatHistoryJson = chatHistoryJson,
                                    ChatEvaluation = evaluationJson,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };

                                context.Results.Add(newResult);
                            }

                            // Lưu thay đổi
                            await context.SaveChangesAsync();
                            // =================================================================================
                        }

                        return Ok(resultData);
                    }
                    else
                    {
                        return StatusCode(500, "Gemini response format invalid.");
                    }
                }
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Error parsing AI response: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }

    private string CleanJsonString(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        text = text.Trim();
        if (text.StartsWith("```json"))
        {
            text = text.Substring(7); // Xóa ```json
        }

        if (text.StartsWith("```"))
        {
            text = text.Substring(3); // Xóa ```
        }

        if (text.EndsWith("```"))
        {
            text = text.Substring(0, text.Length - 3); // Xóa ``` ở cuối
        }

        return text.Trim();
    }
}
public class CreateChattingExerciseDTO
{
    [Required]
    public Guid TopicId { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }

    [Required, MaxLength(255)]
    public string AiRole { get; set; }

    [Required]
    public string Requirement { get; set; } // JSON string array

    [Required]
    public string Objective { get; set; }

    [Required]
    public int MaxAiReplies { get; set; }

    [Required]
    public int Level { get; set; } = 1;

    [Required]
    public string OpeningQuestion { get; set; }

    // Nhận file ảnh từ React
    public IFormFile? Image { get; set; }
}