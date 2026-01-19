using LushEnglishAPI.BackgroundServices;
using LushEnglishAPI.Data;
using LushEnglishAPI.Mapper;
using LushEnglishAPI.Services;
using LushEnglishAPI.Middlewares;
using LushEnglishAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ IHttpClientFactory
builder.Services.AddHttpClient();

// Cho phép service đọc HttpContext (header UserId / SessionId)
builder.Services.AddHttpContextAccessor();

// TopicIdsService (custom service của bạn)
builder.Services.AddScoped<TopicIdsService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHostedService<DailyStreakReminderService>();
builder.Services.AddHostedService<LushEnglishAPI.BackgroundServices.EmailCampaignSchedulerService>();


// Thêm DbContext với DefaultConnection
builder.Services.AddDbContext<LushEnglishDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.Configure<GeminiApiSettings>(
    builder.Configuration.GetSection("GeminiApiSettings"));

builder.Services.Configure<MomoSettings>(
    builder.Configuration.GetSection("MomoSettings"));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Khai báo tên Policy CORS để sử dụng sau này
const string AllowAllPolicy = "AllowAllOrigins";

// Đăng ký dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowAllPolicy, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Thêm Controllers
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Thêm Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseStaticFiles();
app.UseCors(AllowAllPolicy);

// Sử dụng HTTPS
app.UseHttpsRedirection();

// Swagger chỉ bật trong Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<DailyLoginStreakMiddleware>();

// Map Controllers
app.MapControllers();

app.Run();