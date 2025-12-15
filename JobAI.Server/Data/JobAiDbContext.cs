using JobAI.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAI.Server.Data
{
    public class JobAiDbContext : DbContext
    {
        public JobAiDbContext(DbContextOptions<JobAiDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vacancy> Vacancies { get; set; } = null!;
        public DbSet<CoverLetter> CoverLetters { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Vacancy>(entity =>
            {
                entity.ToTable("Vacancies");

                entity.HasKey(v => v.Id);

                entity.Property(v => v.Title)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(v => v.Company)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(v => v.Location)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(v => v.SourceUrl)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(v => v.Description)
                    .IsRequired();
            });


            modelBuilder.Entity<CoverLetter>(entity =>
            {
                entity.ToTable("CoverLetters");

                entity.HasKey(c => c.Id);

                entity.Property(c => c.CandidateName)
                    .HasMaxLength(256);

                entity.Property(c => c.CandidateContacts)
                    .HasMaxLength(256);

                entity.Property(c => c.LetterText)
                    .IsRequired();

                entity.HasOne(c => c.Vacancy)
                    .WithMany(v => v.CoverLetters)
                    .HasForeignKey(c => c.VacancyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

