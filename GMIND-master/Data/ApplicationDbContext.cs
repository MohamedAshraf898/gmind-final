using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Gamingv1.Models;

namespace Gamingv1.Data
{
    /// <summary>
    /// Main database context for the Educational Game Store
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // User-related tables
        public DbSet<OAuthAccount> OAuthAccounts { get; set; }

        // Game-related tables
        public DbSet<GameCategory> GameCategories { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameImage> GameImages { get; set; }
        public DbSet<GameReview> GameReviews { get; set; }
        public DbSet<GamePurchase> GamePurchases { get; set; }
        public DbSet<GameToken> GameTokens { get; set; }

        // Class management tables
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassStudent> ClassStudents { get; set; }
        public DbSet<ParentStudent> ParentStudents { get; set; }
        public DbSet<ParentStudentRelationship> ParentStudentRelationships { get; set; }

        // Event tables
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }

        // Dynamic content tables
        public DbSet<DynamicSection> DynamicSections { get; set; }
        public DbSet<DynamicTopic> DynamicTopics { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure User relationships - Use Restrict to avoid cascade conflicts
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.ParentStudents)
                .WithOne(ps => ps.Parent)
                .HasForeignKey(ps => ps.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.StudentParents)
                .WithOne(ps => ps.Student)
                .HasForeignKey(ps => ps.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure all other User-related relationships to use Restrict
            builder.Entity<OAuthAccount>()
                .HasOne(o => o.User)
                .WithMany(u => u.OAuthAccounts)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GamePurchase>()
                .HasOne(gp => gp.User)
                .WithMany(u => u.GamePurchases)
                .HasForeignKey(gp => gp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GameToken>()
                .HasOne(gt => gt.User)
                .WithMany(u => u.GameTokens)
                .HasForeignKey(gt => gt.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GameReview>()
                .HasOne(gr => gr.User)
                .WithMany(u => u.GameReviews)
                .HasForeignKey(gr => gr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Class>()
                .HasOne(c => c.Teacher)
                .WithMany(u => u.TaughtClasses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClassStudent>()
                .HasOne(cs => cs.Student)
                .WithMany(u => u.ClassStudents)
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EventRegistration>()
                .HasOne(er => er.User)
                .WithMany(u => u.EventRegistrations)
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure GameReview unique constraint
            builder.Entity<GameReview>()
                .HasIndex(gr => new { gr.GameId, gr.UserId })
                .IsUnique();

            // Configure ClassStudent unique constraint
            builder.Entity<ClassStudent>()
                .HasIndex(cs => new { cs.ClassId, cs.StudentId })
                .IsUnique();

            // Configure ParentStudent unique constraint
            builder.Entity<ParentStudent>()
                .HasIndex(ps => new { ps.ParentId, ps.StudentId })
                .IsUnique();

            // Configure EventRegistration unique constraint
            builder.Entity<EventRegistration>()
                .HasIndex(er => new { er.EventId, er.UserId })
                .IsUnique();

            // Configure Class JoinCode unique constraint
            builder.Entity<Class>()
                .HasIndex(c => c.JoinCode)
                .IsUnique();

            // Configure ApplicationUser StudentCode unique constraint
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.StudentCode)
                .IsUnique();

            // Seed data
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed Game Categories
            builder.Entity<GameCategory>().HasData(
                new GameCategory { CategoryId = 1, Name = "Math", Description = "Educational math games", CreatedAt = new DateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Utc) },
                new GameCategory { CategoryId = 2, Name = "Science", Description = "Science learning games", CreatedAt = new DateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Utc) },
                new GameCategory { CategoryId = 3, Name = "Language", Description = "Language learning games", CreatedAt = new DateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Utc) },
                new GameCategory { CategoryId = 4, Name = "History", Description = "Historical educational games", CreatedAt = new DateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Utc) },
                new GameCategory { CategoryId = 5, Name = "Geography", Description = "Geography learning games", CreatedAt = new DateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed Dynamic Sections
            builder.Entity<DynamicSection>().HasData(
                new DynamicSection { SectionId = 1, Name = "Latest Trends", Description = "Latest educational trends", CreatedAt = new DateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Utc) },
                new DynamicSection { SectionId = 2, Name = "Featured Games", Description = "Featured educational games", CreatedAt = new DateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Utc) },
                new DynamicSection { SectionId = 3, Name = "News", Description = "Educational news and updates", CreatedAt = new DateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
