using Microsoft.EntityFrameworkCore;
using Hedonist.Models;

namespace Hedonist.Repositories {
    public class HedonistDbContext : DbContext {
        private readonly string connectionString;
        public DbSet<Question> Question { get; set; }
        public DbSet<Answer> Answer { get; set; }
        public DbSet<Gift> Gift { get; set; }
        public DbSet<GiftType> GiftType { get; set; }
        public DbSet<PasswordInfo> PasswordInfo { get; set; }
        public DbSet<Store> Store { get; set; }
        public DbSet<Terminal> Terminal { get; set; }
        public DbSet<LoginAttempt> LoginAttempt { get; set; }


        public HedonistDbContext(DbContextOptions<HedonistDbContext> options) : base(options) {
        }

        public HedonistDbContext(string connectionString) : base() {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder) {
            if (!builder.IsConfigured) {
                builder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Answer>()
                .HasMany(e => e.GiftTypes)
                .WithMany(e => e.Answers);
        }
    }
}
