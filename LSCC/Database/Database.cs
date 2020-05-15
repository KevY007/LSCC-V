using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using Pomelo.EntityFrameworkCore.MySql.Design;
using System.Reflection;
using GTANetworkAPI;
using System.Diagnostics;
using System.Data.Common;

namespace LSCC
{
    public class MainDatabase : DbContext
    {
        // Base constructor
        public MainDatabase(DbContextOptions options) : base(options)
        {

        }

        // Account model class created somewhere else
        public DbSet<Account> Accounts { get; set; }
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
