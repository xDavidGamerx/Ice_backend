using IceBackend.Domain.Entities;

namespace IceBackend.Domain.Services
{
    /// <summary>
    /// Contrato de dominio para firmar y verificar tokens de equipamiento usando criptografía simétrica.
    /// </summary>
    public interface ICosmeticTokenSigner
    {
        byte[] Sign(ValidatedEquipmentToken token);
        bool Verify(ValidatedEquipmentToken token);
    }
}
