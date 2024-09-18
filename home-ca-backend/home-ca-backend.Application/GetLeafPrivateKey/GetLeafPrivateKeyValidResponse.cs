namespace home_ca_backend.Application.GetLeafPrivateKey;

public record GetLeafPrivateKeyValidResponse(string PrivateKey) : GetLeafPrivateKeyResponse(true);