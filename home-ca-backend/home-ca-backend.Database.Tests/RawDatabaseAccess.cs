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
        
}