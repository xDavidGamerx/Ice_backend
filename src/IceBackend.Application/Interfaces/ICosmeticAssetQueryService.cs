using System;
using System.Threading.Tasks;
using IceBackend.Application.DTOs;

namespace IceBackend.Application.Interfaces
{
    public interface ICosmeticAssetQueryService
    {
        Task<CosmeticAssetDto?> GetCosmeticAssetInfoAsync(Guid id);
    }
}
