using System.Runtime.InteropServices;
using Microsoft.Data.SqlClient;

namespace SP23.P02.Tests.Web.Helpers;

public sealed class SqlServerTestDatabaseProvider
{
    private const string DbPrefix = "SP23-Test-";
    private static string? databaseName;

    public static void AssemblyInit()
    {
        databaseName = $"{DbPrefix}{Guid.NewGuid():N}";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var sqlConnection = new SqlConnection("server=(localdb)\\mssqllocaldb");

            sqlConnection.Open();
            var tempPath = Environment.GetEnvironmentVariable("SqlServerTestDatabaseMdfPath") ?? Path.GetTempPath();
            var mdfPath = $"{tempPath}{databaseName}".Replace("'", "''");
            var sql = $@"
        CREATE DATABASE
            [{databaseName}]
        ON PRIMARY (
           NAME=Test_data,
           FILENAME = '{mdfPath}.mdf'
        )
        LOG ON (
            NAME=Test_log,
            FILENAME = '{mdfPath}.ldf'
        )";

            using var command = new SqlCommand(sql, sqlConnection);
            command.ExecuteNonQuery();
        }
        else
        {
            var master = GetConnection("master");
            using var sqlConnection = new SqlConnection(master);

            sqlConnection.Open();

            using var command = new SqlCommand($"CREATE DATABASE [{databaseName}]", sqlConnection);
            command.ExecuteNonQuery();
        }
    }

    public static void ApplicationCleanup()
    {
        DeleteDatabase();
    }

    public static string GetConnectionString()
    {
        if (databaseName == null)
        {
            throw new Exception("SQL not configured");
        }
        return GetConnection(databaseName);
    }

    /// <summary>
    /// Clears all data in the database but leaves the scheme and objects alone.
    /// Note: Ids will start from higher numbers on tests that run in sequence for the same assembly init
    /// </summary>
    public static void ClearData()
    {
        if (databaseName == null)
        {
            return;
        }

        var master = GetConnection("master");
        using var sqlConnection = new SqlConnection(master);

        sqlConnection.Open();
        //see: https://stackoverflow.com/a/49735672/1590723
        using var command = new SqlCommand($@"
USE [{databaseName}]

EXEC sp_MSForEachTable 'DISABLE TRIGGER ALL ON ?'


EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'


EXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON; IF (''?'' <> ''[dbo].[__EFMigrationsHistory]'' AND ''?'' <> ''[HangFire].[Schema]'') DELETE FROM ?'


EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'


EXEC sp_MSForEachTable 'ENABLE TRIGGER ALL ON ?'
", sqlConnection);
        command.ExecuteNonQuery();
    }

    private static string GetConnection(string name)
    {
        string connection;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            connection = $"Server=localhost,1433;Database={name};User Id=sa;Password=Password123!;TrustServerCertificate=True";
        }
        else
        {
            connection = $"Server=(localdb)\\mssqllocaldb;Database={name};Trusted_Connection=True";
        }

        return connection;
    }

    private static void DeleteDatabase()
    {
        var master = GetConnection("master");
        using var sqlConnection = new SqlConnection(master);

        sqlConnection.Open();
        using var command = new SqlCommand(@$"
IF EXISTS(select * from sys.databases where name='{databaseName}')
BEGIN
    ALTER DATABASE  [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
END

DROP DATABASE IF EXISTS [{databaseName}];
", sqlConnection);
        command.ExecuteNonQuery();
        databaseName = null;
    }
}
