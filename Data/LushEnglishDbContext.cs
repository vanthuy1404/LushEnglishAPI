// 2025
// DANGTHUY

using Microsoft.EntityFrameworkCore;
using LushEnglishAPI.Models; // assuming your models namespace

namespace LushEnglishAPI.Data
{
    public class LushEnglishDbContext(DbContextOptions<LushEnglishDbContext> options) : DbContext(options)
    {
        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set; }
        public DbSet<Practice> Practices { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<ChattingConfig> ChattingConfigs { get; set; }
        public DbSet<WritingConfig> WritingConfigs { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
