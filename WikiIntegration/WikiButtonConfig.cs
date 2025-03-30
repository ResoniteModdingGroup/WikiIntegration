using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.UI.Inspectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace WikiIntegration
{
    /// <summary>
    /// Contains settings for the Resonite Wiki buttons on components, ProtoFlux nodes, and Component Selector categories.
    /// </summary>
    public sealed class WikiButtonConfig : SingletonConfigSection<WikiButtonConfig>
    {
        private static readonly DefiningConfigKey<bool> _componentCategories = new("ComponentCategories", "Whether to show Wiki buttons on Categories of Components in Component Selectors when <i>Component Selector Additions</i> is available.", () => true)
        {
            new ConfigKeySessionShare<bool>(true)
        };

        private static readonly DefiningConfigKey<int> _componentOffset = new("ComponentOffset", "The Order Offset of the Wiki button on Inspector Headers. Higher is further right.", () => 2)
        {
            DefaultInspectorHeaderConfig.OffsetRange,
            DefaultInspectorHeaderConfig.MakeOffsetRangeShare(2)
        };

        private static readonly DefiningConfigKey<bool> _components = new("Components", "Whether to show Wiki buttons on Components in Worker Inspectors.", () => true)
        {
            new ConfigKeySessionShare<bool>(true)
        };

        private static readonly DefiningConfigKey<bool> _protoFlux = new("ProtoFlux", "Whether to show Wiki buttons on ProtoFlux nodes.", () => true)
        {
            new ConfigKeySessionShare<bool>(true)
        };

        private static readonly DefiningConfigKey<bool> _protoFluxCategories = new("ProtoFluxCategories", "Whether to show Wiki buttons on Categories of ProtoFlux nodes in Component Selectors and ProtoFlux Node Browsers when <i>Component Selector Additions</i> is available.", () => true)
        {
            new ConfigKeySessionShare<bool>(true)
        };

        /// <summary>
        /// Gets the session share for whether Resonite Wiki buttons on component categories in Component Selectors should be visible.
        /// </summary>
        public ConfigKeySessionShare<bool> ComponentCategories => _componentCategories.Components.Get<ConfigKeySessionShare<bool>>();

        /// <summary>
        /// Gets the session share for the Order Offset of the Resonite Wiki buttons on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<int, long> ComponentOffset => _componentOffset.Components.Get<ConfigKeySessionShare<int, long>>();

        /// <summary>
        /// Gets the session share for whether Resonite Wiki buttons on Inspector Headers should be visible.
        /// </summary>
        public ConfigKeySessionShare<bool> Components => _components.Components.Get<ConfigKeySessionShare<bool>>();

        /// <inheritdoc/>
        public override string Description => "Contains settings for the Resonite Wiki buttons on components, ProtoFlux nodes, and Component Selector categories.";

        /// <inheritdoc/>
        public override string Id => "Buttons";

        /// <summary>
        /// Gets the session share for whether Resonite Wiki buttons on any ProtoFlux nodes should be visible.
        /// </summary>
        public ConfigKeySessionShare<bool> ProtoFlux => _protoFlux.Components.Get<ConfigKeySessionShare<bool>>();

        /// <summary>
        /// Gets the session share for whether Resonite Wiki buttons on ProtoFlux categories in Component Selectors / ProtoFlux Node Browsers should be visible.
        /// </summary>
        public ConfigKeySessionShare<bool> ProtoFluxCategories => _protoFluxCategories.Components.Get<ConfigKeySessionShare<bool>>();

        /// <inheritdoc/>
        public override Version Version { get; } = new(1, 0, 0);
    }
}