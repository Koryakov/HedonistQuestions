using Microsoft.EntityFrameworkCore;
using Hedonist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonist.Repositories {
    public class DbProvider {
		public string ConnectionString { get; set; } = null;
        private DbContextOptions<HedonistDbContext> dbOptions;

        public DbProvider() {
            dbOptions = new DbContextOptions<HedonistDbContext>();
        }

        public HedonistDbContext CreateContext() {
            if (ConnectionString != null) {
                return new HedonistDbContext(ConnectionString);
            }
            else {
                return new HedonistDbContext(dbOptions);
            }
        }
    }
}
