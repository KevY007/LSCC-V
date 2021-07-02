using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace LSCC
{
    public class Language
    {
        [Key]
        public int SQL_ID { get; set; }
        public string LanguageName { get; set; }
    }

    public class MapSpawn
    {
        [Key]
        public int SQL_ID { get; set; }
        public int mapID { get; set; }

        public int spawnTeam { get; set; }
        public float spawnX { get; set; }
        public float spawnY { get; set; }
        public float spawnZ { get; set; }
        public float spawnA { get; set; }
    }
    public class Map
    {
        [Key]
        public int SQL_ID { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

    public class BanData
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountSQLID { get; set; }
        public ulong RockstarID { get; set; }
        public string IP { get; set; }
        public string HWID { get; set; }
        public string Reason { get; set; }
        public string Admin { get; set; }
        public string DateTime { get; set; }
    }

    public class MainDatabase : DbContext
    {
        // Base constructor
        public MainDatabase(DbContextOptions options) : base(options)
        {

        }

        // Account model class created somewhere else
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Map> Maps { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<MapSpawn> MapSpawns { get; set; }
        public DbSet<BanData> Bans { get; set; }
        public DbSet<Logs_AdminRecord> Logs_AdminRecords { get; set; }
        public DbSet<Logs_Admin> Logs_Admin { get; set; }
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
            if (File.Exists("lscc/we_are_testing"))
                _connectionString = "Server=" + Settings.MySQL_Host + ";Database=" + "lsccv_test" + ";Uid=" + Settings.MySQL_User + ";Pwd=" + Settings.MySQL_Password;
            else 
                _connectionString = "Server=" + Settings.MySQL_Host + ";Database=" + Settings.MySQL_Database + ";Uid=" + Settings.MySQL_User + ";Pwd=" + Settings.MySQL_Password;
        }

    }
}
