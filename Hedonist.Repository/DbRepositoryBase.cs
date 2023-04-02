

namespace Hedonist.Repositories {
    public class DbRepositoryBase {
        private readonly string connectionString;

        public DbRepositoryBase(string connectionString) {
            this.connectionString = connectionString;
        }

        protected HedonistDbContext CreateContext() {
            return new HedonistDbContext(connectionString);
        }
    }

}
