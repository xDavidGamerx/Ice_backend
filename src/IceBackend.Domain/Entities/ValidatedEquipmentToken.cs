using System;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    /// <summary>
    /// Certificado de equipamiento firmado criptográficamente con tiempo de expiración.
    /// Transporta el ID interno para evitar consultas en el repositorio (guardado O(1)).
    /// </summary>
    public record ValidatedEquipmentToken(
        PlayerId PlayerId, 
        CosmeticId CosmeticId, 
        CosmeticType Slot,
        int CosmeticInternalId, // ID subrogado interno para la base de datos (Shadow Property)
        int AssetVersion,
        DateTime ExpiredAt,
        byte[] Signature // HMAC-SHA256
    );
}
