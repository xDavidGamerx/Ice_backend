using System;

namespace IceBackend.Domain.Entities
{
    /// <summary>
    /// Identificador inmutable fuertemente tipado para la entidad Player.
    /// Contiene operadores de conversión implícita para garantizar la interoperabilidad
    /// transparente con el resto del backend (Guid) sin requerir refactorizaciones masivas.
    /// </summary>
    public record PlayerId
    {
        public Guid Value { get; }

        public PlayerId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("El identificador del jugador no puede estar vacío.", nameof(value));
            
            Value = value;
        }

        public override string ToString() => Value.ToString();

        // Operadores de conversión implícita para interoperabilidad limpia
        public static implicit operator Guid(PlayerId playerId) => playerId?.Value ?? Guid.Empty;
        public static implicit operator PlayerId(Guid value) => new(value);
    }
}
