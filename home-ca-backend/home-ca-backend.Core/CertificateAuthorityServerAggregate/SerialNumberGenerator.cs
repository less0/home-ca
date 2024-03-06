using System.Security.Cryptography;

namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public static class SerialNumberGenerator
{
    public static byte[] GenerateSerialNumber()
    {
        byte[] serialNumber = RandomNumberGenerator.GetBytes(16);

        // Set the MSB to 0 to ensure the serial number is positive
        serialNumber[0] &= 0x7F;

        return serialNumber;
    }
}