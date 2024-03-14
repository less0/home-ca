using System.Data;
using home_ca_backend.Core;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
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

    public T? GetReferenceValueByTableAndId<TTable, T>(Id id, string columnName)
        where T : class
    {
        using SqlConnection connection = new(Constants.DatabaseConnectionString);
        using SqlCommand command = connection.CreateCommand();

        connection.Open();
        command.CommandText = $"SELECT {columnName} FROM {typeof(TTable).Name} WHERE Id=@id";
        command.Parameters.AddWithValue("id", id.Guid);

        var result = command.ExecuteScalar();
        return result is DBNull
            ? null
            : result as T;
    }
    
    public T? GetValueByTableAndId<TTable, T>(Id id, string columnName)
        where T : struct
    {
        using SqlConnection connection = new(Constants.DatabaseConnectionString);
        using SqlCommand command = connection.CreateCommand();

        connection.Open();
        command.CommandText = $"SELECT {columnName} FROM {typeof(TTable).Name} WHERE Id=@id";
        command.Parameters.AddWithValue("id", id.Guid);

        var result = command.ExecuteScalar();
        return result is DBNull
            ? null
            : (T)result;
    }

    public void ClearDatabase()
    {
        using SqlConnection connection = new(Constants.DatabaseConnectionString);
        using SqlCommand command = connection.CreateCommand();

        connection.Open();
        command.CommandText = $"DELETE FROM {nameof(CertificateAuthority)}";
        command.ExecuteNonQuery();

        command.CommandText = $"DELETE FROM {nameof(Leaf)}";
        command.ExecuteNonQuery();
    }

    public void CreateNestedCertificateAuthorities(int nesting)
    {
        using SqlConnection connection = new(Constants.DatabaseConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText =
            $"INSERT INTO {nameof(CertificateAuthority)} (Id, CertificateAuthorityId, Name, CreatedAt) VALUES (@Id, @ParentId, @Name, @CreatedAt)";
        var idParameter = command.Parameters.Add("Id", SqlDbType.UniqueIdentifier);
        var parentIdParameter = command.Parameters.Add("ParentId", SqlDbType.UniqueIdentifier);
        var nameParameter = command.Parameters.AddWithValue("Name", SqlDbType.Text);
        var createdAtParameter = command.Parameters.Add("CreatedAt", SqlDbType.DateTime2);
        
        connection.Open();
        Guid? parentId = null;
        for (int level = 0; level <= nesting; level++)
        {
            Guid id = Guid.NewGuid();

            idParameter.Value = id;
            parentIdParameter.Value = (object?)parentId ?? DBNull.Value;
            nameParameter.Value = $"Level {level}";
            createdAtParameter.Value = DateTime.UtcNow;

            command.ExecuteNonQuery();
            
            parentId = id;
        }
    }
}