#pragma warning disable CS1591
namespace PerAspera.GameAPI.Economy.Models
{
    /// <summary>Types d'entités terrestres (pays, corporations).</summary>
    public enum EarthEntityType
    {
        Country,
        Corporation
    }

    /// <summary>Direction d'un échange commercial.</summary>
    public enum TradeDirection
    {
        /// <summary>Export depuis Mars vers la Terre.</summary>
        MarsToEarth,
        /// <summary>Import depuis la Terre vers Mars.</summary>
        EarthToMars
    }

    /// <summary>
    /// Palier de réputation avec une entité terrestre.
    /// Les seuils exacts sont configurables via <see cref="ReputationScore.Thresholds"/>.
    /// </summary>
    public enum ReputationTier
    {
        Hostile = 0,
        Wary    = 1,
        Neutral = 2,
        Friendly = 3,
        Allied  = 4
    }
}
#pragma warning restore CS1591
