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
        public DbSet<Course> Courses { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<UserDailyLoginStreak> UserDailyLoginStreaks { get; set; }
        public DbSet<EmailCampaign> EmailCampaigns { get; set; }
        public DbSet<EmailCampaignDelivery> EmailCampaignDeliveries { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserDailyLoginStreak>(entity =>
            {
                // Ensure ActivityDate stored as DATE (no time)
                entity.Property(x => x.ActivityDate)
                    .HasColumnType("date");

                // Unique: 1 record per user per day
                entity.HasIndex(x => new { x.UserId, x.ActivityDate })
                    .IsUnique();

                // Optional FK to Users (recommended)
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                modelBuilder.Entity<EmailCampaignDelivery>()
                    .HasIndex(x => new { x.CampaignId, x.UserId })
                    .IsUnique();
            });
        }

    }
}
