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
public class UserController(LushEnglishDbContext context, IMapper mapper, IWebHostEnvironment environment) : ControllerBase
{
    private readonly LushEnglishDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IWebHostEnvironment _environment = environment;

    // ==========================================
    // PHẦN 1: CRUD CƠ BẢN (Quản lý User)
    // ==========================================

    // GET: api/User
    [HttpGet]
    public async Task<ActionResult<List<UserInfoDTO>>> GetAllUsers()
    {
        // Chỉ lấy list user cơ bản, không kèm Results để nhẹ response
        var users = await _context.Users.ToListAsync();
        var result = _mapper.Map<List<UserInfoDTO>>(users);
        return Ok(result);
    }

    // GET: api/User/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserInfoDTO>> GetUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) 
            return NotFound("User not found");

        var result = _mapper.Map<UserInfoDTO>(user);
        return Ok(result);
    }

    // POST: api/User
    [HttpPost]
    public async Task<ActionResult<UserInfoDTO>> CreateUser([FromBody] UserInfoDTO userDto)
    {
        var newUser = new User
        {
            Id = Guid.CreateVersion7(),
            FullName = userDto.FullName,
            Email = userDto.Email,
            AvatarUrl = userDto.AvatarUrl,
            CreatedAt = DateTime.UtcNow.AddHours(7)
            // Lưu ý: Password nên được xử lý hash riêng, không truyền trực tiếp qua DTO này nếu có
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<UserInfoDTO>(newUser);
        return Ok(result);
    }

    // PUT: api/User/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<UserInfoDTO>> UpdateUser(Guid id, [FromBody] UserInfoDTO userDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) 
            return NotFound("User not found");

        user.FullName = userDto.FullName;
        user.AvatarUrl = userDto.AvatarUrl;
        // user.Email thường hạn chế cho phép sửa đổi tùy nghiệp vụ
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<UserInfoDTO>(user);
        return Ok(result);
    }

    // DELETE: api/User/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) 
            return NotFound("User not found");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ==========================================
    // PHẦN 2: API INFO CHO TRANG CÁ NHÂN
    // ==========================================

    // GET: api/User/info/{id}
    [HttpGet("info/{id}")]
    [SessionCheck]
    public async Task<ActionResult<UserInfoDTO>> GetFullUserInfo(Guid id)
    {
        // Lấy userId từ header (Key phải khớp với cái bạn set trong axiosService: 'userId')
        string headerUserId = Request.Headers["userId"].FirstOrDefault();

        // Kiểm tra xem header có tồn tại không và có khớp với userId trong body gửi lên không
        // Sử dụng Equals để so sánh chuỗi an toàn
        if (string.IsNullOrEmpty(headerUserId) || !headerUserId.Equals(id.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            // Trả về 401 Unauthorized nếu không khớp
            return Unauthorized(new { message = "User ID mismatch or missing in headers." });
        }
        
        // 1. Lấy thông tin User
        var user = await _context.Users.FindAsync(id);
        if (user == null) 
            return NotFound("User not found");

        var userInfo = _mapper.Map<UserInfoDTO>(user);

        // 2. Lấy danh sách kết quả (Results) của user, sắp xếp mới nhất trước
        var results = await _context.Results
            .Where(r => r.UserId == id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        if (results.Any())
        {
            // --- A. TÍNH TOÁN THỐNG KÊ (STATS) ---
            
            // Tổng điểm tích lũy
            userInfo.TotalExp = results.Sum(r => r.Score ?? 0);
            
            // Điểm trung bình chung
            userInfo.AverageScore = results.Average(r => r.Score ?? 0);

            // Điểm trung bình từng kỹ năng (tránh chia cho 0)
            
            // Type 1: Multiple Choice
            var type1Results = results.Where(r => r.PracticeType == 1).ToList();
            userInfo.AverageScoreMultipleChoice = type1Results.Any() 
                ? type1Results.Average(r => r.Score ?? 0) : 0;

            // Type 2: Writing
            var type2Results = results.Where(r => r.PracticeType == 2).ToList();
            userInfo.AverageScoreWriting = type2Results.Any() 
                ? type2Results.Average(r => r.Score ?? 0) : 0;

            // Type 3: Chatting
            var type3Results = results.Where(r => r.PracticeType == 3).ToList();
            userInfo.AverageScoreChatting = type3Results.Any() 
                ? type3Results.Average(r => r.Score ?? 0) : 0;


            // --- B. MAP TÊN BÀI TẬP (Lookup Name) ---
            
            var resultDtos = _mapper.Map<List<UserResultDTO>>(results);

            // Bước tối ưu: Gom ID để query tên bài tập 1 lần (tránh N+1 query)
            var practiceIds = results.Where(r => r.PracticeType == 1).Select(r => r.TargetId).Distinct().ToList();
            var writingIds = results.Where(r => r.PracticeType == 2).Select(r => r.TargetId).Distinct().ToList();
            var chattingIds = results.Where(r => r.PracticeType == 3).Select(r => r.TargetId).Distinct().ToList();

            // Truy vấn lấy Dictionary [Id, Name]
            var practiceNames = await _context.Practices
                .Where(p => practiceIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name); 

            var writingNames = await _context.WritingConfigs
                .Where(w => writingIds.Contains(w.Id))
                .ToDictionaryAsync(w => w.Id, w => w.Name);

            var chattingNames = await _context.ChattingConfigs
                .Where(c => chattingIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            // Gán tên vào DTO
            foreach (var item in resultDtos)
            {
                if (item.PracticeType == 1 && practiceNames.TryGetValue(item.TargetId, out var pName))
                {
                    item.PracticeName = pName;
                }
                else if (item.PracticeType == 2 && writingNames.TryGetValue(item.TargetId, out var wName))
                {
                    item.PracticeName = wName;
                }
                else if (item.PracticeType == 3 && chattingNames.TryGetValue(item.TargetId, out var cName))
                {
                    item.PracticeName = cName;
                }
                else
                {
                    item.PracticeName = "Unknown/Deleted Exercise";
                }
            }

            userInfo.Results = resultDtos;
        }
        else 
        {
            // Nếu User mới tạo chưa làm bài nào, trả về 0 hết
            userInfo.TotalExp = 0;
            userInfo.AverageScore = 0;
            userInfo.AverageScoreMultipleChoice = 0;
            userInfo.AverageScoreWriting = 0;
            userInfo.AverageScoreChatting = 0;
            userInfo.Results = new List<UserResultDTO>();
        }

        return Ok(userInfo);
    }
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDTO request)
    {
        // 1. Tìm User
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null) 
            return NotFound(new { message = "User not found" });

        // 2. Cập nhật tên
        user.FullName = request.FullName;

        // 3. Xử lý Avatar nếu có upload
        if (request.Avatar != null && request.Avatar.Length > 0)
        {
            // Đường dẫn gốc tới thư mục wwwroot
            string webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                // Fallback nếu chưa cấu hình WebRootPath (mặc định là thư mục hiện tại/wwwroot)
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // Tạo thư mục images nếu chưa có
            string uploadDir = Path.Combine(webRootPath, "images");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            // Tạo tên file: tenanh_shortguid.duoi
            string originalName = Path.GetFileNameWithoutExtension(request.Avatar.FileName);
            string extension = Path.GetExtension(request.Avatar.FileName);
            // Lấy 8 ký tự đầu của Guid để làm short guid
            string shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8); 
            
            // Validate tên file an toàn (bỏ ký tự đặc biệt nếu cần), ở đây giữ đơn giản
            string newFileName = $"{originalName}_{shortGuid}{extension}";

            string filePath = Path.Combine(uploadDir, newFileName);

            // Lưu file vào ổ cứng
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Avatar.CopyToAsync(stream);
            }

            // Cập nhật đường dẫn vào DB (bắt đầu bằng /images/...)
            user.AvatarUrl = $"/images/{newFileName}";
        }

        // 4. Lưu DB
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = "Profile updated successfully", 
            fullName = user.FullName,
            avatarUrl = user.AvatarUrl 
        });
    }
}