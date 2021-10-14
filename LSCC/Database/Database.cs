using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace RNC
{
    public class AdminNote
    {
        public enum ANoteType
        {
            None = 0,
            OnConnect = 1,
            OnSpectate = 2,
            Both
        };

        [Key]
        public int SQL_ID { get; set; }
        public int AccountID { get; set; }
        public int AdminID { get; set; }
        public string Timestamp { get; set; }
        public string Text { get; set; }
        public ANoteType Type { get; set; }
        public string TypeToString()
        {
            switch (this.Type)
            {
                case ANoteType.None: return "None";
                case ANoteType.OnConnect: return "On Connect";
                case ANoteType.OnSpectate: return "On Spectate";
                case ANoteType.Both: return "Both";
                default: return "None";
            }
        }
    }
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
        public DateTime DateTime { get; set; }
    }
    public class PlayerOutfit
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountID { get; set; }
        public string OutfitData { get; set; }
    }
    public class OwnedVehData
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountSQLID { get; set; }
        public string Model { get; set; }
        public int Team { get; set; }
        public int TimesUsed { get; set; }
    }
    public class PlayerTunings
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountSQLID { get; set; }
        public uint Model { get; set; }
        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }
        public int PearlescentColor { get; set; }
        public int WheelColor { get; set; }
        public string Mods { get; set; }
    }
    public class OwnedLoadoutData
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountSQLID { get; set; }
        public string Name { get; set; }
        public int Team { get; set; }
        public int TimesUsed { get; set; }
    }
    public class CustomDMArenas
    {
        [Key]
        public int SQL_ID { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; } // String so if the account is deleted, the DM arena still remains.
        public int TimesEntered { get; set; }
        public string IPLs { get; set; }
        public string Extras { get; set; }
    }
    public class CustomDMArenaTeams
    {
        [Key]
        public int SQL_ID { get; set; }
        public int ArenaID { get; set; }
        public string Name { get; set; }
        public int Team { get; set; }
        public int Health { get; set; }
        public int Armour { get; set; }
        public bool Blip { get; set; }
        public int BlipColor { get; set; }
        public int ColorR { get; set; }
        public int ColorG { get; set; }
        public int ColorB { get; set; }
        public int Alpha { get; set; }
        public int ShowHPBars { get; set; } // 0 = Never, 1 = Always, 2 = On Aim, 3 = Busted!
        public int SharedSpawns { get; set; }

        public string WeaponSets { get; set; }
    }
    public class CustomDMArenaSpawns
    {
        [Key]
        public int SQL_ID { get; set; }
        public int ArenaID { get; set; }
        public int Team { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float A { get; set; }
    }

    public class DM_WeaponData
    {
        public long Weapon;
        public int Ammo;
        public long[] Components;
    }

    public class DM_WeaponSets
    {
        public DM_WeaponData[] Weapons;
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
        public DbSet<PlayerOutfit> PlayerOutfits { get; set; }
        public DbSet<PlayerTunings> PlayerTunings { get; set; }
        public DbSet<OwnedVehData> OwnedVehicles { get; set; }
        public DbSet<OwnedLoadoutData> OwnedLoadouts { get; set; }
        public DbSet<AdminNote> AdminNotes { get; set; }
        public DbSet<Logs_AdminRecord> Logs_AdminRecords { get; set; }
        public DbSet<Logs_Admin> Logs_Admin { get; set; }
        public DbSet<Logs_Session> Logs_Session { get; set; }
        public DbSet<CustomDMArenas> CustomArenas { get; set; }
        public DbSet<CustomDMArenaTeams> CustomArenas_Teams { get; set; }
        public DbSet<CustomDMArenaSpawns> CustomArenas_Spawns { get; set; }
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
            builder.UseMySql(_connectionString, optionsBuilder => {
                optionsBuilder.MigrationsAssembly(typeof(MainDatabase).GetTypeInfo().Assembly.GetName().Name);
                optionsBuilder.EnableRetryOnFailure();
                optionsBuilder.CommandTimeout(60); // Default: 30s
            });
           
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
            if (File.Exists("racenchase/we_are_testing"))
                _connectionString = "Server=" + Settings.MySQL_Host + ";Database=" + "racenchase_test" + ";Uid=" + Settings.MySQL_User + ";Pwd=" + Settings.MySQL_Password;
            else 
                _connectionString = "Server=" + Settings.MySQL_Host + ";Database=" + Settings.MySQL_Database + ";Uid=" + Settings.MySQL_User + ";Pwd=" + Settings.MySQL_Password;
        }

    }
}
