using Microsoft.EntityFrameworkCore;
using Hedonist.Models;

namespace Hedonist.Repositories {
    public class HedonistDbContext : DbContext {
        private readonly string connectionString;
        //public DbSet<TgMessage> TgMessage { get; set; }

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
    }
}
