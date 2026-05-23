using System;
using IceBackend.Domain.Entities;

namespace IceBackend.Domain.Services
{
    /// <summary>
    /// Contrato de dominio para firmar y verificar tokens de asignación de rangos.
    /// </summary>
    public interface IRangeTokenSigner
    {
        byte[] SignRange(PlayerId playerId, string rangeType, DateTime? expiresAt, string eventId);
        bool VerifyRange(ValidatedRangeAssignmentToken token);
    }
}
