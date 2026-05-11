using System.Data;
using DeviceMgmt.Repository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DeviceMgmt.Repository.Core;

public class UnitWork : IUnitWork
{
    private readonly string _connectionString;

    public UnitWork(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
    }

    public IDbConnection OpenConnection()
    {
        var conn = new SqlConnection(_connectionString);
        conn.Open();
        return conn;
    }
}
