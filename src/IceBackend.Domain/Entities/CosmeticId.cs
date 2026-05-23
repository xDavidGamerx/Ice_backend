using System;

namespace IceBackend.Domain.Entities
{
    /// <summary>
    /// Identificador inmutable fuertemente tipado para CosmeticAsset.
    /// Contiene operadores de conversión implícita para garantizar la interoperabilidad
    /// transparente con el resto del backend (Guid) sin requerir refactorizaciones masivas.
    /// </summary>
    public record CosmeticId
    {
        public Guid Value { get; }

        public CosmeticId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("El identificador del cosmético no puede estar vacío.", nameof(value));
            
            Value = value;
        }

        public override string ToString() => Value.ToString();

        // Operadores de conversión implícita para interoperabilidad limpia
        public static implicit operator Guid(CosmeticId cosmeticId) => cosmeticId?.Value ?? Guid.Empty;
        public static implicit operator CosmeticId(Guid value) => new(value);
    }
}
