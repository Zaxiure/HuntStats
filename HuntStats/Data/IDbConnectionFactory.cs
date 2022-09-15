using System.Data.Common;
using Dapper;
using Dommel;
using HuntStats.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace HuntStats.Data;

public interface IDbConnectionFactory
{
    DbConnection GetOpenConnection();
    Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default(CancellationToken));
}

public class ConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public ConnectionFactory(IConfiguration configuration) => _configuration = configuration;

    public DbConnection GetOpenConnection()
    {
        DbConnection connection = GetConnection();
        connection.Open();
        return connection;
    }

    public async Task<DbConnection> GetOpenConnectionAsync(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        DbConnection connection = GetConnection();
        await connection.OpenAsync(cancellationToken);
        DbConnection openConnectionAsync = connection;
        return openConnectionAsync;
    }

    private DbConnection GetConnection()
    {
        var oldFilePath = "HuntStats.sqlite";
        var newFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HuntStats/HuntStats.sqlite";
        if (File.Exists(oldFilePath))
        {
            File.Copy(oldFilePath, newFilePath);
            Task.Delay(500);
            File.Delete(oldFilePath);
        }
        DbConnection connection = new SqliteConnection($"Data Source={newFilePath}");
        return connection;
    }
}