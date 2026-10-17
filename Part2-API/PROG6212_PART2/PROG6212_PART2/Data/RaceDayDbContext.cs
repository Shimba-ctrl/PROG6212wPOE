
using Microsoft.EntityFrameworkCore;
using PROG6212_PART2.Models;

namespace PROG6212_PART2.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(
            DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PROG6212_PART2.Models.Route> Routes { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users: email must be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Users: only Organiser or Participant roles are allowed
            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Users_Role",
                    "[Role] IN ('Organiser', 'Participant')"));

            // Events: each event belongs to one organiser
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany()
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Categories: each category belongs to one event
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Routes: one route per event
            modelBuilder.Entity<PROG6212_PART2.Models.Route>()
                .HasOne(r => r.Event)
                .WithOne(e => e.Route)
                .HasForeignKey<PROG6212_PART2.Models.Route>(
                    r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrolments: link participants to categories
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany()
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // A participant can enrol in a category only once
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantId, e.CategoryId })
                .IsUnique();

            // Enrolments: only Confirmed or Cancelled statuses
            modelBuilder.Entity<Enrolment>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Enrolments_Status",
                    "[Status] IN ('Confirmed', 'Cancelled')"));

            // Results: one result per enrolment
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Results: record the organiser who captured the result
            modelBuilder.Entity<Result>()
                .HasOne(r => r.CapturedByOrganiser)
                .WithMany()
                .HasForeignKey(r => r.CapturedByOrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // An enrolment cannot have duplicate results
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentId)
                .IsUnique();

            // Finish time uses SQL Server TIME
            modelBuilder.Entity<Result>()
                .Property(r => r.FinishTime)
                .HasColumnType("time");

            // Decimal precision for distances and elevation
            modelBuilder.Entity<Category>()
                .Property(c => c.Distance)
                .HasPrecision(6, 2);

            modelBuilder.Entity<PROG6212_PART2.Models.Route>()
                .Property(r => r.DistanceKm)
                .HasPrecision(6, 2);

            modelBuilder.Entity<PROG6212_PART2.Models.Route>()
                .Property(r => r.ElevationGain)
                .HasPrecision(6, 2);
        }
    }
}


