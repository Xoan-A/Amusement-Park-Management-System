using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<Attraction> Attractions { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<VisitorReport> VisitorReports { get; set; }
        public virtual DbSet<Report> Reports { get; set; }

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ignore old inheritance-based classes (backward compatibility during migration)
            modelBuilder.Ignore<Administrator>();
            modelBuilder.Ignore<Operator>();
            modelBuilder.Ignore<Visitor>();

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Role configuration
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // UserRole configuration (many-to-many)
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId);

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = Role.ADMINISTRATOR },
                new Role { Id = 2, Name = Role.OPERATOR },
                new Role { Id = 3, Name = Role.VISITOR }
            );

            // Attraction configuration
            modelBuilder.Entity<Attraction>()
                .HasIndex(a => a.Name)
                .IsUnique();

            // Event configuration
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.Name)
                .IsUnique();

            modelBuilder.Entity<EventAttraction>()
                .HasKey(ea => new { ea.EventId, ea.AttractionId });

            modelBuilder.Entity<EventAttraction>()
                .HasOne(ea => ea.Event)
                .WithMany(e => e.Attractions)
                .HasForeignKey(ea => ea.EventId);

            modelBuilder.Entity<EventAttraction>()
                .HasOne(ea => ea.Attraction)
                .WithMany()
                .HasForeignKey(ea => ea.AttractionId);

            // Ticket configuration
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.QRCode)
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Visitor)
                .WithMany()
                .HasForeignKey(t => t.VisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VisitorReport>()
                .HasOne(vr => vr.Visitor)
                .WithMany(v => v.VisitorReports)
                .HasForeignKey(vr => vr.VisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.VisitorReport)
                .WithMany(vr => vr.Reports)
                .HasForeignKey(r => r.VisitorReportId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Attraction)
                .WithMany()
                .HasForeignKey(r => r.AttractionId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
