using FrooxEngine;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using System.Reflection;

namespace WikiIntegration
{
    internal sealed class ProtoFluxCategoryConfig : ExpandoConfigSection
    {
        internal const string ProtoFluxPath = "ProtoFlux/Runtimes/Execution/Nodes";

        private static readonly Dictionary<string, ConfigKeySessionShare<bool>> _sessionSharesByCategory = [];

        /// <inheritdoc/>
        public override string Description => "Contains settings for the Resonite Wiki buttons on specific categories of ProtoFlux nodes.";

        /// <inheritdoc/>
        public override string Id => "ProtoFluxCategories";

        public ConfigKeySessionShare<bool>? this[string category]
        {
            get
            {
                _sessionSharesByCategory.TryGetValue(category, out var sessionShare);
                return sessionShare;
            }
        }

        public ConfigKeySessionShare<bool>? this[Type worker]
        {
            get
            {
                if (worker.GetCustomAttribute<CategoryAttribute>() is not CategoryAttribute category)
                    return null;

                foreach (var path in category.Paths)
                {
                    if (this[path] is ConfigKeySessionShare<bool> sessionShare)
                        return sessionShare;
                }

                return null;
            }
        }

        public override Version Version { get; } = new(1, 0, 0);

        public static string GetToggleId(string categoryPath)
            => $"Category-{categoryPath.Replace('/', '_')}";

        internal void Initialize()
        {
            var protoFluxNodesRoot = WorkerInitializer.ComponentLibrary.GetSubcategory(ProtoFluxPath);
            CreateConfigKeys(protoFluxNodesRoot, ProtoFluxPath);
        }

        private void CreateConfigKeys(CategoryNode<Type> category, string path)
        {
            if (category.Elements.Any())
            {
                IEntity<IDefiningConfigKey<bool>> categoryKey = CreateDefiningKey(new ConfigKey<bool>(GetToggleId(path)), $"Whether to show the Resonite Wiki button on ProtoFlux nodes in the category {path}.", () => true);
                var sessionShare = new ConfigKeySessionShare<bool>(true);
                categoryKey.Components.Add(sessionShare);
                _sessionSharesByCategory.Add(path, sessionShare);
            }

            foreach (var subcategory in category.Subcategories)
                CreateConfigKeys(subcategory, $"{path}/{subcategory.Name}");
        }
    }
}