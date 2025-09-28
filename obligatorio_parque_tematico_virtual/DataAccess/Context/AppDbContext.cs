using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Administrator>("Administrator")
                .HasValue<Operator>("Operator")
                .HasValue<Visitor>("Visitor");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.QRCode)
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Visitor)
                .WithMany()
                .HasForeignKey(t => t.VisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}