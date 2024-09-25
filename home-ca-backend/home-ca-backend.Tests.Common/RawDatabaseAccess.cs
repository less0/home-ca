using System.Data;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using home_ca_backend.Core;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Microsoft.Data.SqlClient;

namespace home_ca_backend.Tests.Common;

public class RawDatabaseAccess(string connectionString)
{
    public T? GetReferenceValueByTableAndId<TTable, T>(Id id, string columnName)
        where T : class
    {
        using SqlConnection connection = new(connectionString);
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
        using SqlConnection connection = new(connectionString);
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
        using SqlConnection connection = new(connectionString);
        using SqlCommand command = connection.CreateCommand();

        connection.Open();
        command.CommandText = $"DELETE FROM {nameof(Leaf)}";
        command.ExecuteNonQuery();
        
        command.CommandText = $"DELETE FROM {nameof(CertificateAuthority)}";
        command.ExecuteNonQuery();
    }

    public void CreateNestedCertificateAuthorities(int nesting)
    {
        using SqlConnection connection = new(connectionString);
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

    public void CreateRootCertificateAuthorityWithCertificate(Guid id, string name, string certificate)
    {
        using SqlConnection connection = new(connectionString);
        using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {nameof(CertificateAuthority)} (Id, Name, CreatedAt, PemCertificate) " +
                              $"VALUES (@Id, @Name, @CreatedAt, @PemCertificate)";
        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("Name", name);
        command.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("PemCertificate", certificate);
        connection.Open();

        command.ExecuteNonQuery();
    }

    public void CreateRootCertificateAuthorityWithPrivateKey(Guid id, string name, string privateKey)
    {
        using SqlConnection connection = new(connectionString);
        using var command = connection.CreateCommand();

        command.CommandText = $"INSERT INTO {nameof(CertificateAuthority)} (Id, Name, CreatedAt, PemPrivateKey) " +
                              $"VALUES (@Id, @Name, @CreatedAt, @PemPrivateKey)";
        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("Name", name);
        command.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("PemPrivateKey", privateKey);
        connection.Open();
        
        command.ExecuteNonQuery();
    }

    public void CreateRootCertificateAuthority(Guid id, string name)
    {
        using SqlConnection connection = new(connectionString);
        using var command = connection.CreateCommand();

        command.CommandText = $"INSERT INTO {nameof(CertificateAuthority)} (Id, Name, CreatedAt) " +
                              $"VALUES (@Id, @Name, @CreatedAt)";
        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("Name", name);
        command.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public void CreateIntermediateCertificateAuthority(Guid parentId, Guid id, string name)
    {
        using SqlConnection connection = new(connectionString);
        using var command = connection.CreateCommand();

        command.CommandText = $"INSERT INTO {nameof(CertificateAuthority)} (Id, Name, CreatedAt, CertificateAuthorityId) " +
                              $"VALUES (@Id, @Name, @CreatedAt, @ParentId)";
        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("Name", name);
        command.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("ParentId", parentId);
        
        connection.Open();
        command.ExecuteNonQuery();
    }

    public void CreateCertificateForRootCertificateAuthority(Guid id, string password = "123456")
    {
        using SqlConnection connection = new(connectionString);
        using SqlCommand command = connection.CreateCommand();

        command.CommandText = $"SELECT Name FROM {nameof(CertificateAuthority)} WHERE Id=@Id";
        command.Parameters.AddWithValue("Id", id);

        connection.Open();

        var name = command.ExecuteScalar() as string;
        if (name == null)
        {
            throw new Exception($"{nameof(CertificateAuthority)} with ID {id} not found");
        }

        using RSA rsa = RSA.Create(4096);
        CertificateRequest certificateRequest = new(new X500DistinguishedName($"cn={name}"), rsa, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: true, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));
        var certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(10));

        command.CommandText = $"UPDATE {nameof(CertificateAuthority)} SET " +
                              $"{nameof(CertificateAuthority.EncryptedCertificate)}=@Certificate, " +
                              $"{nameof(CertificateAuthority.PemCertificate)}=@PemCertificate, " +
                              $"{nameof(CertificateAuthority.PemPrivateKey)}=@PemPrivateKey " +
                              $"WHERE Id=@Id";
        command.Parameters.AddWithValue("Certificate", certificate.Export(X509ContentType.Pkcs12, password));
        command.Parameters.AddWithValue("PemCertificate", certificate.ExportCertificatePem());
        command.Parameters.AddWithValue("PemPrivateKey", certificate.ExportEncryptedPkcs8PrivateKeyPem(password));
        command.ExecuteNonQuery();
    }

    public void CreateLeaf(Guid certificateAuthorityId, Guid leafId, string leafName)
    {
        using SqlConnection connection = new(connectionString);
        using var command = connection.CreateCommand();

        connection.Open();

        command.CommandText =
            $"INSERT INTO {nameof(Leaf)} ({nameof(Leaf.Id)}, {nameof(Leaf.Name)}, {nameof(Leaf.CreatedAt)}, CertificateAuthorityId) " +
            $"VALUES (@Id, @Name, @CreatedAt, @CertificateAuthorityId)";
        command.Parameters.AddWithValue("Id", leafId);
        command.Parameters.AddWithValue("Name", leafName);
        command.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("CertificateAuthorityId", certificateAuthorityId);

        command.ExecuteNonQuery();
    }

    public void CreateCertificateForLeaf(Guid leafId, string leafPassword, string certificateAuthorityPassword)
    {
        var certificateAuthorityId = GetParentCertificateAuthorityId(leafId);
        var certificate = LoadCertificate(certificateAuthorityId, certificateAuthorityPassword);
        var leafName = GetLeafName(leafId);
        var leafCertificate = CreateLeafCertificate(certificate, leafName);
        StoreLeafCertificate(leafId, leafCertificate, leafPassword);
    }

    public void CreateCertificateForLeaf(Guid leafId, string leafPassword)
    {
        var leafName = GetLeafName(leafId);
        var certificate = CreateSelfSignedCertificate(leafName);
        
        StoreLeafCertificate(leafId, certificate, leafPassword);
    }

    private X509Certificate2 CreateSelfSignedCertificate(string name)
    {
        using RSA rsa = RSA.Create(4096);
        CertificateRequest certificateRequest = new(new X500DistinguishedName($"cn={name}"), rsa, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: true, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));
        var certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(365));

        return certificate;
    }

    private Guid GetParentCertificateAuthorityId(Guid leafId)
    {
        using SqlConnection connection = new(connectionString);
        using var command = connection.CreateCommand();

        connection.Open();

        command.CommandText = $"SELECT CertificateAuthorityId FROM {nameof(Leaf)} " +
                              $"WHERE {nameof(Leaf.Id)}=@Id";
        command.Parameters.AddWithValue("Id", leafId);

        return (Guid)command.ExecuteScalar()!;
    }

    private string GetLeafName(Guid leafId)
    {
        using SqlConnection connection = new(connectionString);
        using var command = connection.CreateCommand();

        connection.Open();

        command.CommandText = $"SELECT {nameof(Leaf.Name)} FROM {nameof(Leaf)} " +
                              $"WHERE {nameof(Leaf.Id)}=@Id";
        command.Parameters.AddWithValue("Id", leafId);

        return (string)command.ExecuteScalar()!;
    }

    private X509Certificate2 LoadCertificate(Guid certificateAuthorityId, string certificateAuthorityPassword)
    {
        using SqlConnection connection = new(connectionString);
        using var command = connection.CreateCommand();
        connection.Open();
        command.CommandText =
            $"SELECT {nameof(CertificateAuthority.EncryptedCertificate)} FROM {nameof(CertificateAuthority)} " +
            $"WHERE {nameof(CertificateAuthority.Id)}=@Id";
        command.Parameters.AddWithValue("Id", certificateAuthorityId);
        var bytes = (byte[])command.ExecuteScalar()!;
        return new(bytes, certificateAuthorityPassword);
    }

    private X509Certificate2 CreateLeafCertificate(X509Certificate2 signingCertificate, string leafName)
    {
        RSA rsa = RSA.Create();
        CertificateRequest certificateRequest =
            new(new X500DistinguishedName($"cn={leafName}"), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        certificateRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DataEncipherment, false));
        return certificateRequest.Create(signingCertificate, signingCertificate.NotBefore, signingCertificate.NotAfter,
            BitConverter.GetBytes(Random.Shared.NextInt64()))
            .CopyWithPrivateKey(rsa);
    }

    private void StoreLeafCertificate(Guid leafId, X509Certificate2 leafCertificate, string leafPassword)
    {
        using SqlConnection connection = new(connectionString);
        using var command = connection.CreateCommand();

        connection.Open();

        command.CommandText = $"UPDATE {nameof(Leaf)} SET " +
                              $"{nameof(Leaf.PemCertificate)}=@PemCertificate, " +
                              $"{nameof(Leaf.PemPrivateKey)}=@PemPrivateKey, " +
                              $"{nameof(Leaf.EncryptedCertificate)}=@Certificate " +
                              $"WHERE {nameof(Leaf.Id)}=@Id";
        command.Parameters.AddWithValue("Id", leafId);
        command.Parameters.AddWithValue("PemCertificate", leafCertificate.ExportCertificatePem());
        command.Parameters.AddWithValue("PemPrivateKey",
            leafCertificate.ExportEncryptedPkcs8PrivateKeyPem(leafPassword));
        command.Parameters.AddWithValue("Certificate", leafCertificate.Export(X509ContentType.Pfx, leafPassword));

        command.ExecuteNonQuery();
    }
}