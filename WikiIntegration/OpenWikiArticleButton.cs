using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Events;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.Locale;
using MonkeyLoader.Resonite.UI;
using MonkeyLoader.Resonite.UI.Inspectors;
using ProtoFlux.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiIntegration
{
    /// <summary>
    /// This monkey must not be disabled to generate wiki buttons for everyone else in the session.
    /// </summary>
    [HarmonyPatchCategory(nameof(OpenWikiArticleButton))]
    [HarmonyPatch(typeof(ProtoFluxNodeVisual), nameof(ProtoFluxNodeVisual.GenerateVisual))]
    internal sealed class OpenWikiArticleButton : ConfiguredResoniteInspectorMonkey<OpenWikiArticleButton, WikiButtonConfig, BuildInspectorHeaderEvent, Worker>,
        IAsyncEventHandler<FallbackLocaleGenerationEvent>
    {
        private static readonly Lazy<LocaleString> _componentLocale = new(() => Mod.GetLocaleString("WikiHyperlink.Component"));
        private static readonly Lazy<LocaleString> _protoFluxLocale = new(() => Mod.GetLocaleString("WikiHyperlink.ProtoFlux"));
        private static ProtoFluxCategoryConfig _categoryConfig = null!;
        private static readonly Dictionary<string, string> _nameOverrides = new() {
            {"dT", "Delta_Time"},
            {"ObjectCast", "Object_Cast"},
            {"ValueCast", "Value_Cast"}
        };

        public override int Priority => HarmonyLib.Priority.HigherThanNormal;

        private static LocaleString ComponentLocale => _componentLocale.Value;

        private static LocaleString ProtoFluxLocale => _protoFluxLocale.Value;

        public Task Handle(FallbackLocaleGenerationEvent eventData)
        {
            var toggleDescription = eventData.GetMessage(Mod.GetLocaleKey("ProtoFluxCategoryToggles.Description"));

            var protoFluxNodesRoot = WorkerInitializer.ComponentLibrary.GetSubcategory(ProtoFluxCategoryConfig.ProtoFluxPath);
            CreateConfigKeyNames(eventData, toggleDescription, protoFluxNodesRoot, ProtoFluxCategoryConfig.ProtoFluxPath);

            return Task.CompletedTask;
        }

        protected override void Handle(BuildInspectorHeaderEvent eventData)
        {
            var ui = eventData.UI;

            ui.PushStyle();
            ui.Style.FlexibleWidth = 0;
            ui.Style.MinWidth = 40;

            var button = ui.Button(OfficialAssets.Graphics.Badges.Mentor)
                .WithTooltip(eventData.Worker is ProtoFluxNode ? ProtoFluxLocale : ComponentLocale);

            AddHyperlink(button.Slot, eventData.Worker);

            ConfigSection.Components.DriveFromVariable(button.Slot.ActiveSelf_Field);
            ConfigSection.ComponentOffset.DriveFromVariable(button.Slot._orderOffset);

            ui.PopStyle();
        }

        protected override bool OnEngineReady()
        {
            _categoryConfig = Config.LoadSection<ProtoFluxCategoryConfig>();
            _categoryConfig.Initialize();

            Mod.RegisterEventHandler<FallbackLocaleGenerationEvent>(this);

            return base.OnEngineReady();
        }

        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventHandler<FallbackLocaleGenerationEvent>(this);

            return base.OnShutdown(applicationExiting);
        }

        private static void AddHyperlink(Slot slot, Worker worker)
        {
            string wikiPage;
            LocaleString reason;

            if (worker is ProtoFluxNode node)
            {
                reason = ProtoFluxLocale;
                var nodeName = node.NodeName;

                var nodeMetadata = NodeMetadataHelper.GetMetadata(node.NodeType);
                if (!string.IsNullOrEmpty(nodeMetadata.Overload))
                {
                    var overload = nodeMetadata.Overload;
                    var dotIndex = overload.LastIndexOf('.');

                    nodeName = dotIndex > 0 ? overload[(dotIndex + 1)..] : nodeName;
                }

                if (_nameOverrides.TryGetValue(nodeName, out var name))
                    nodeName = name; 

                wikiPage = $"ProtoFlux:{nodeName.Replace(' ', '_')}";
            }
            else
            {
                reason = ComponentLocale;
                var workerName = worker.WorkerType.Name;

                // Don't need to remove the `1 on generics - they redirect and may actually be different
                wikiPage = $"Component:{workerName}";
            }

            var hyperlink = slot.AttachComponent<Hyperlink>();
            hyperlink.URL.Value = new Uri($"https://wiki.resonite.com/{wikiPage}");
            hyperlink.Reason.AssignLocaleString(reason);
        }

        private static void Postfix(ProtoFluxNodeVisual __instance, ProtoFluxNode node)
        {
            if (!Engine.IsAprilFools && node.SupressHeaderAndFooter && node.NodeName.Contains("Relay", StringComparison.OrdinalIgnoreCase))
                return;

            var ui = new UIBuilder(__instance.LocalUIBuilder.Canvas);

            var buttonArea = ui.Panel();
            ui.IgnoreLayout();
            ConfigSection.ProtoFlux.DriveFromVariable(buttonArea.Slot.ActiveSelf_Field);

            buttonArea.AnchorMin.Value = new(1, 0);
            buttonArea.AnchorMax.Value = new(1, 0);
            buttonArea.OffsetMin.Value = new(-12, 2);
            buttonArea.OffsetMax.Value = new(-2, 12);

            // creates texture for every button
            var button = ui.Image(OfficialAssets.Graphics.Badges.Mentor);
            button.Slot.AttachComponent<Button>().WithTooltip(ProtoFluxLocale);

            AddHyperlink(button.Slot, node);

            if (_categoryConfig[node.GetType()] is ConfigKeySessionShare<bool> categoryShare)
                categoryShare.DriveFromVariable(button.Slot.ActiveSelf_Field);
        }

        private void CreateConfigKeyNames(FallbackLocaleGenerationEvent eventData, string descriptionFormat, CategoryNode<Type> category, string path)
        {
            if (category.Elements.Any())
            {
                var trimmedPath = path.Replace(ProtoFluxCategoryConfig.ProtoFluxPath, "");

                var id = $"{_categoryConfig.FullId}.{ProtoFluxCategoryConfig.GetToggleId(path)}";
                var description = string.Format(descriptionFormat, trimmedPath);

                eventData.AddMessage($"{id}.Name", trimmedPath);
                eventData.AddMessage($"{id}.Description", description);
            }

            foreach (var subcategory in category.Subcategories)
                CreateConfigKeyNames(eventData, descriptionFormat, subcategory, $"{path}/{subcategory.Name}");
        }
    }
}