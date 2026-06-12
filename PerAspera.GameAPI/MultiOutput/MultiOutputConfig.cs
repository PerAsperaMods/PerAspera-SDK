using System.Collections.Generic;

namespace PerAspera.GameAPI.MultiOutput
{
    /// <summary>
    /// DTO of one item of the <c>multiOutput</c> section in a mod's <c>sdk.yaml</c>:
    /// the extra outputs a building type produces on top of its native single output.
    /// </summary>
    /// <example>
    /// # Mods/MonMod/sdk.yaml
    /// extensions:
    ///   multiOutput:
    ///     building_water_mine:
    ///       extraOutputs:
    ///         - resource: resource_carbon
    ///           quantity: 1
    ///           scaleWithProductivity: true
    /// </example>
    public sealed class MultiOutputConfig
    {
        /// <summary>Extra outputs produced at each completed production cycle.</summary>
        public List<ExtraOutputDef> ExtraOutputs { get; set; } = new();
    }

    /// <summary>One extra output declaration (YAML or code-registered).</summary>
    /// <example>new ExtraOutputDef { Resource = "resource_carbon", Quantity = 1f }</example>
    public sealed class ExtraOutputDef
    {
        /// <summary>Resource key in the datamodel (string — resolved post-load, no !tag).</summary>
        public string Resource { get; set; } = "";

        /// <summary>Units produced per completed production cycle (before scaling).</summary>
        public float Quantity { get; set; } = 1f;

        /// <summary>Scale the quantity by the factory's current productivity (default true).</summary>
        public bool ScaleWithProductivity { get; set; } = true;
    }
}
