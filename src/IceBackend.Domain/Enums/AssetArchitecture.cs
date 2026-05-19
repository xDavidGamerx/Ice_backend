namespace IceBackend.Domain.Enums
{
    /// <summary>
    /// Arquitectura de destino del asset en el ecosistema ICE Launcher.
    ///
    /// - Legacy   : Minecraft 1.8.x — modelos OBJ planos, sin Optifine CIT avanzado.
    /// - Modern   : Fabric 1.21+    — modelos BBMODEL (Blockbench), animaciones GeckoLib.
    /// - Universal: Texturas 2D (capas, logos) compatibles con ambas arquitecturas.
    /// </summary>
    public enum AssetArchitecture
    {
        Legacy,
        Modern,
        Universal
    }
}
