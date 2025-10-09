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
        public virtual DbSet<StrategyConfiguration> StrategyConfigurations { get; set; }
        public virtual DbSet<DateTimeConfiguration> DateTimeConfigurations { get; set; }

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

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

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = Role.ADMINISTRATOR },
                new Role { Id = 2, Name = Role.OPERATOR },
                new Role { Id = 3, Name = Role.VISITOR }
            );

            var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var operatorId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminId,
                    Name = "Admin",
                    LastName = "User",
                    Email = "admin@test.com",
                    Password = "$2a$11$TgMvdaYlj5ZLE7ybPvoFi.jqSqp6S39yr3JXz34wo/ReZThKeHuYq", // admin123
                    BirthDate = new DateTime(1980, 1, 1),
                    MembershipLevel = MembershipLevel.VIP
                },
                new User
                {
                    Id = operatorId,
                    Name = "Operator",
                    LastName = "User",
                    Email = "operator@test.com",
                    Password = "$2a$11$QHVhQ21m/dB3cntgTO2aqu3SNiQn6d7nUnRE3lPE4LPEoFJGRSEJu", // operator123
                    BirthDate = new DateTime(1985, 1, 1),
                    MembershipLevel = MembershipLevel.Standard
                }
            );

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = adminId, RoleId = 1 },
                new UserRole { UserId = operatorId, RoleId = 2 }
            );

            modelBuilder.Entity<Attraction>()
                .HasIndex(a => a.Name)
                .IsUnique();

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

            modelBuilder.Entity<StrategyConfiguration>().HasData(
                new StrategyConfiguration
                {
                    Id = 1,
                    StrategyName = "PerAttraction",
                    N = null,
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
