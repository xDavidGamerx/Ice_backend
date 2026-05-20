using System;

namespace IceBackend.Application.Interfaces
{
    public interface IAssetTokenService
    {
        string GenerateDeliveryToken(Guid playerId, string hash, TimeSpan ttl);
        bool ValidateDeliveryToken(string token, string expectedHash, out Guid playerId);
    }
}
