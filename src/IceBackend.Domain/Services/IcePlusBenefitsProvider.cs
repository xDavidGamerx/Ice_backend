using System;
using System.Collections.Generic;

namespace IceBackend.Domain.Services
{
    /// <summary>
    /// Representa los beneficios activos de la suscripción ICE+ de un jugador.
    /// </summary>
    public record IcePlusBenefits(
        string Prefix,
        string Color,
        string IconColor,
        bool HasClothCloaks,
        bool NoAds,
        bool UnlimitedFriends,
        decimal DiscountPercentage,
        IReadOnlyCollection<Guid> ExclusiveCosmeticIds
    );

    /// <summary>
    /// Proveedor de dominio que define estáticamente los beneficios de la suscripción ICE+
    /// y calcula las variantes del icono evolutivo basado en meses acumulados.
    /// </summary>
    public static class IcePlusBenefitsProvider
    {
        private static readonly Guid DefaultDanceEmoteId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid ExclusiveCapeId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid ExclusiveBackpackId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        private static readonly Guid ExclusiveBandannaId = Guid.Parse("00000000-0000-0000-0000-000000000004");

        private static readonly List<Guid> ExclusiveCosmeticIds = new()
        {
            DefaultDanceEmoteId,
            ExclusiveCapeId,
            ExclusiveBackpackId,
            ExclusiveBandannaId
        };

        /// <summary>
        /// Resuelve y retorna los beneficios correspondientes a los meses acumulados del jugador.
        /// </summary>
        public static IcePlusBenefits GetBenefits(int accumulatedMonths)
        {
            string iconColor = ResolveIconColor(accumulatedMonths);

            return new IcePlusBenefits(
                Prefix: "[ICE+]",
                Color: "#FF69B4", // Rosa distintivo del servicio premium
                IconColor: iconColor,
                HasClothCloaks: true,
                NoAds: true,
                UnlimitedFriends: true,
                DiscountPercentage: 10.0m, // 10% de descuento automático en la tienda
                ExclusiveCosmeticIds: ExclusiveCosmeticIds.AsReadOnly()
            );
        }

        /// <summary>
        /// Resuelve el color del icono evolutivo (15 variantes) según la cantidad de meses acumulados.
        /// El suscriptor recibe un icono rosa con signo más verde por defecto.
        /// </summary>
        private static string ResolveIconColor(int months)
        {
            if (months < 2) return "pink-green"; // Nivel 1 (Por defecto)
            return months switch
            {
                2 => "blue",          // Nivel 2
                3 => "green",         // Nivel 3
                4 => "red",           // Nivel 4
                5 => "yellow",        // Nivel 5
                6 => "purple",        // Nivel 6
                7 => "cyan",          // Nivel 7
                8 => "orange",        // Nivel 8
                9 => "gray",          // Nivel 9
                10 => "white",        // Nivel 10
                11 => "black",        // Nivel 11
                12 => "gold",         // Nivel 12 (1 año)
                18 => "diamond",      // Nivel 13 (1.5 años)
                24 => "emerald",      // Nivel 14 (2 años)
                _ when months >= 36 => "rainbow", // Nivel 15 (3 años o más)
                _ => ResolveInterpolatedColor(months) // Fallback inteligente para meses intermedios
            };
        }

        /// <summary>
        /// Resuelve los meses intermedios de forma determinista para mantenerse en el nivel anterior alcanzado.
        /// </summary>
        private static string ResolveInterpolatedColor(int months)
        {
            if (months > 24) return "emerald";
            if (months > 18) return "diamond";
            if (months > 12) return "gold";
            return "orange"; // Fallback general
        }
    }
}
