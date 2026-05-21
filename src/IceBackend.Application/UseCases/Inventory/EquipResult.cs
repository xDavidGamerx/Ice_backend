namespace IceBackend.Application.UseCases.Inventory
{
    /// <summary>
    /// Resultado tipado de la operación de equipar un cosmético.
    /// Evita excepciones para flujos de control previsibles.
    /// </summary>
    public enum EquipResult
    {
        Success,
        PlayerNotFound,
        CosmeticNotFound,
        NotOwned,
        ArchitectureIncompatible,
        SlotMismatch,
        ArchitectureNotProvided
    }
}
