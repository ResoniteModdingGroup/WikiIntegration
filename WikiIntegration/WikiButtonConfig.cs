using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace WikiIntegration
{
    internal sealed class WikiButtonConfig : ConfigSection
    {
        private static readonly DefiningConfigKey<bool> _components = new("Components", "Whether to add the Wiki button to Components in Worker Inspectors.", () => true);
        private static readonly DefiningConfigKey<bool> _protoFlux = new("ProtoFlux", "Whether to add the Wiki button to ProtoFlux nodes.", () => true);

        private readonly DefiningConfigKey<int> _componentOffset = new("ComponentOffset", "The Order Offset of the Wiki button on Inspector Headers. Higher is further right.", () => 2)
        {
            new ConfigKeyRange<int>(0, 16), // Replace with DefaultInspectorHeaderConfig.OffsetRange
            new ConfigKeySessionShare<int, long>(i => i, l => (int)l, 2)
        };

        /// <summary>
        /// Gets the Order Offset share for the Resonite Wiki button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<int, long> ComponentOffset => _componentOffset.Components.Get<ConfigKeySessionShare<int, long>>();

        public bool Components => _components;

        public override string Description => "Contains settings for the Resonite Wiki buttons on components and ProtoFlux nodes.";
        public override string Id => "Buttons";
        public bool ProtoFlux => _protoFlux;
        public override Version Version { get; } = new(1, 0, 0);
    }
}