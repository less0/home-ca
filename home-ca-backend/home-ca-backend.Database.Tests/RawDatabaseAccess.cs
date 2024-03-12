﻿using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Microsoft.Data.SqlClient;

namespace home_ca_backend.Database.Tests;

public class RawDatabaseAccess
{
    public void WaitForConnection()
    {
        SqlConnection sqlConnection = new SqlConnection(Constants.ConnectionString);
        bool isConnected = false;
        DateTime connectionAttemptStartedAt = DateTime.Now;
        
        while (!isConnected)
        {
            try
            {
                sqlConnection.Open();
                isConnected = true;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
                if (DateTime.Now - connectionAttemptStartedAt > TimeSpan.FromMinutes(2))
                {
                    throw;
                }
                Thread.Sleep(TimeSpan.FromSeconds(15));
            }
        }
    }

    public T? GetReferenceValueByTableAndId<T>(string tableName, Guid id, string columnName)
        where T : class
    {
        using SqlConnection connection = new(Constants.DatabaseConnectionString);
        using SqlCommand command = connection.CreateCommand();

        connection.Open();
        command.CommandText = $"SELECT {columnName} FROM {tableName} WHERE Id=@id";
        command.Parameters.AddWithValue("id", id);

        var result = command.ExecuteScalar();
        return result is DBNull
            ? null
            : result as T;
    }
    
    public T? GetValueByTableAndId<T>(string tableName, Guid id, string columnName)
        where T : struct
    {
        using SqlConnection connection = new(Constants.DatabaseConnectionString);
        using SqlCommand command = connection.CreateCommand();

        connection.Open();
        command.CommandText = $"SELECT {columnName} FROM {tableName} WHERE Id=@id";
        command.Parameters.AddWithValue("id", id);

        var result = command.ExecuteScalar();
        return result is DBNull
            ? null
            : (T)result;
    }
}