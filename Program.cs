using LushEnglishAPI.Data;
using LushEnglishAPI.Mapper;
using LushEnglishAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Thêm dịch vụ IHttpClientFactory
builder.Services.AddHttpClient();
// Thêm DbContext với DefaultConnection
builder.Services.AddDbContext<LushEnglishDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
);
builder.Services.Configure<GeminiApiSettings>(
    builder.Configuration.GetSection("GeminiApiSettings"));
// Khai báo tên Policy CORS để sử dụng sau này
const string AllowAllPolicy = "AllowAllOrigins";

// THÊM: Đăng ký dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowAllPolicy,
        policy =>
        {
            // Cho phép bất kỳ domain nào (Origin) truy cập
            policy.AllowAnyOrigin()
                // Cho phép bất kỳ HTTP Method nào (GET, POST, PUT, DELETE,...)
                .AllowAnyMethod()
                // Cho phép bất kỳ Header nào
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

// Map Controllers
app.MapControllers();

app.Run();