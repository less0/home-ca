namespace home_ca_backend.Application.GetLeafPrivateKey;

public record GetLeafPrivateKeyResponse(bool IsValid)
{
    public static GetLeafPrivateKeyResponse Valid(string privateKey) => new GetLeafPrivateKeyValidResponse(privateKey);
    public static GetLeafPrivateKeyResponse UnknownLeafId() => new GetLeafPrivateKeyLeafNotFoundResult();
    public static GetLeafPrivateKeyResponse MissingCertificate() => new GetLeafPrivateKeyMissingCertificateResult();
}

