using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.UI.Inspectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace WikiIntegration
{
    internal sealed class WikiButtonConfig : ConfigSection
    {
        private static readonly DefiningConfigKey<bool> _components = new("Components", "Whether to show the Wiki button on Components in Worker Inspectors.", () => true)
        {
            new ConfigKeySessionShare<bool>(true)
        };

        private static readonly DefiningConfigKey<bool> _protoFlux = new("ProtoFlux", "Whether to show the Wiki button on ProtoFlux nodes.", () => true)
        {
            new ConfigKeySessionShare<bool>(true)
        };

        private readonly DefiningConfigKey<int> _componentOffset = new("ComponentOffset", "The Order Offset of the Wiki button on Inspector Headers. Higher is further right.", () => 2)
        {
            DefaultInspectorHeaderConfig.OffsetRange,
            DefaultInspectorHeaderConfig.MakeOffsetRangeShare(2)
        };

        /// <summary>
        /// Gets the Order Offset share for the Resonite Wiki button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<int, long> ComponentOffset => _componentOffset.Components.Get<ConfigKeySessionShare<int, long>>();

        public ConfigKeySessionShare<bool> Components => _components.Components.Get<ConfigKeySessionShare<bool>>();

        public override string Description => "Contains settings for the Resonite Wiki buttons on components and ProtoFlux nodes.";

        public override string Id => "Buttons";

        public ConfigKeySessionShare<bool> ProtoFlux => _protoFlux.Components.Get<ConfigKeySessionShare<bool>>();

        public override Version Version { get; } = new(1, 0, 0);
    }
}