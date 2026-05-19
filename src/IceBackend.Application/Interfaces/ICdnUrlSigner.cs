using System;

namespace IceBackend.Application.Interfaces
{
    public interface ICdnUrlSigner
    {
        (string Url, DateTime ExpiresAt) GeneratePresignedUrl(string assetHash);
    }
}
