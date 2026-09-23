using Microsoft.EntityFrameworkCore;
using KariyerNet.Domain.Entities;

namespace KariyerNet.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
      
       public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users => Set<User>();
        public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
        public DbSet<JobPosting> JobPostings => Set<JobPosting>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja=>ja.Candidate)
                .WithMany()
                .HasForeignKey(ja => ja.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.JobPosting)
                .WithMany()
                .HasForeignKey(ja => ja.JobPostingId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<JobPosting>()
                .HasOne(jp => jp.Employer)
                .WithMany()
                .HasForeignKey(jp => jp.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CandidateProfile>()
                .HasOne(cp => cp.User)
                .WithMany()
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CandidateProfile>()
                .HasIndex(cp => cp.UserId)
                .IsUnique();
            modelBuilder.Entity<JobApplication>()
                .HasIndex(ja => new { ja.CandidateId, ja.JobPostingId })
                .IsUnique();
            base.OnModelCreating(modelBuilder);
        }

    }
}
