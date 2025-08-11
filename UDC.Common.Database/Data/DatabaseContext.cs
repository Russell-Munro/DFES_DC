using System;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using UDC.Common.Database.Data.Models.Database;

namespace UDC.Common.Database.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Connection> Connections { get; set; }
        public DbSet<ConnectionRule> ConnectionRules { get; set; }
        public DbSet<DataConnectorLog> DataConnectorLogs { get; set; }
        public DbSet<UIUser> UIUsers { get; set; }
        public DbSet<ApplicationState> ApplicationStates { get; set; }


        // Add this constructor for EF Core DI
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        public DatabaseContext() {  }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString()); 
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

        }

        public static String GetConnectionString()
        {
            return AppSettings.GetValue("ConnectionStrings:DataConnector");
        }
        public Int32 UpdateSQL(String sqlQuery)
        {
            return this.Database.ExecuteSqlRaw(sqlQuery);
        }

        public String GetCreationScript()
        {
            String retVal = "";
            Assembly objAssembly = typeof(DatabaseContext).GetTypeInfo().Assembly;
            Stream objStream = objAssembly.GetManifestResourceStream("UDC.Common.Database.Data.Scripts.CreateDBSchema.sql");
            StreamReader objReader = new StreamReader(objStream);

            retVal = objReader.ReadToEnd();

            objReader = null;
            objStream = null;
            objAssembly = null;

            return retVal;
        }
        public void CreateDatabaseSchema()
        {
            String strSQL = GetCreationScript();

            if(!String.IsNullOrEmpty(strSQL))
            {
                UpdateSQL(strSQL);
            }
            
            strSQL = null;
        }

        public void CleanupLogs()
        {
            String strSQL = "";
            Int64 intLogLimitDays = GeneralHelpers.parseInt64(AppSettings.GetValue("Logging:PurgeLogsOlderThanDays"));
            Int64 intMinLogsToKeep = GeneralHelpers.parseInt64(AppSettings.GetValue("Logging:MinLogsToKeep"));

            if (intLogLimitDays > 0 && intMinLogsToKeep > 0)
            {
                strSQL = "DECLARE @logCursor AS CURSOR;\n" +
                    "DECLARE @connectionRuleID AS BIGINT;\n" +
                    "SET @logCursor = CURSOR FOR\n" +
                    "SELECT connectionRuleID FROM [equ_dc_DataConnectorLog] GROUP BY connectionRuleID;\n" +
                    "OPEN @logCursor;\n" +
                    "FETCH NEXT FROM @logCursor INTO @connectionRuleID;\n" +
                    "WHILE @@FETCH_STATUS = 0\n" +
                    "BEGIN\n" +
                    "	WITH logSet AS(SELECT ROW_NUMBER() OVER(ORDER BY DateCreated DESC) AS rownumber, DATEDIFF(day,[DateCreated], GETUTCDATE()) AS DiffDays, * FROM [equ_dc_DataConnectorLog] WHERE connectionRuleID = @connectionRuleID)\n" +
                    "	DELETE FROM logSet WHERE rownumber > @MinLogsToKeep AND DATEDIFF(day,[DateCreated], GETUTCDATE()) > @LogLimitDays;\n" +
                    "	FETCH NEXT FROM @logCursor INTO @connectionRuleID;\n" +
                    "END\n" +
                    "CLOSE @logCursor;\n" +
                    "DEALLOCATE @logCursor;\n";
                this.Database.ExecuteSqlRaw(strSQL,
                    new SqlParameter("@MinLogsToKeep", intMinLogsToKeep),
                    new SqlParameter("@LogLimitDays", intLogLimitDays));
            }
        }
    }
}