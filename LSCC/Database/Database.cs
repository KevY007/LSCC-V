using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LSCC
{
    public class MapSpawns
    {
        [Key]
        public int SQL_ID { get; set; }
        public int mapID { get; set; }

        public float spawnX { get; set; }
        public float spawnY { get; set; }
        public float spawnZ { get; set; }
        public float spawnA { get; set; }
    }
    public class Maps
    {
        [Key]
        public int SQL_ID { get; set; }
        public string mapName { get; set; }
    }

    public class MainDatabase : DbContext
    {
        // Base constructor
        public MainDatabase(DbContextOptions options) : base(options)
        {

        }

        // Account model class created somewhere else
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Maps> Maps { get; set; }
        public DbSet<MapSpawns> MapSpawns { get; set; }
    }

    public class ContextFactory : IDesignTimeDbContextFactory<MainDatabase>
    {
        private static MainDatabase _instance;

        private static string _connectionString;

        public MainDatabase CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<MainDatabase>();
          
            // Load the connection string for the first time
            if (string.IsNullOrEmpty(_connectionString))
            {
                LoadConnectionString();
            }
            // Use it to init the connection
            builder.UseMySql(_connectionString, optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(MainDatabase).GetTypeInfo().Assembly.GetName().Name));
           
            return new MainDatabase(builder.Options);
        }

        public static MainDatabase Instance
        {
            get
            {
                if (_instance != null) return _instance;

                return _instance = new ContextFactory().CreateDbContext(new string[] { });
            }
            private set { }
        }

        private static void LoadConnectionString()
        {
            _connectionString = "Server=" + Settings.MySQL_Host + ";Database=" + Settings.MySQL_Database + ";Uid=" + Settings.MySQL_User + ";Pwd=" + Settings.MySQL_Password;
        }

    }
}
