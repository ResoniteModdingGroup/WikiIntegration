using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI;
using MonkeyLoader.Resonite.UI.Inspectors;
using ProtoFlux.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace WikiIntegration
{
    /// <summary>
    /// This monkey must not be disabled to generate wiki buttons for anyone else in the session.
    /// </summary>
    [HarmonyPatchCategory(nameof(OpenWikiArticleButton))]
    [HarmonyPatch(typeof(ProtoFluxNodeVisual), nameof(ProtoFluxNodeVisual.GenerateVisual))]
    internal sealed class OpenWikiArticleButton : ConfiguredResoniteInspectorMonkey<OpenWikiArticleButton, WikiButtonConfig, BuildInspectorHeaderEvent, Worker>
    {
        private static readonly Lazy<LocaleString> _componentLocale = new(() => Mod.GetLocaleString("WikiHyperlink.Component"));
        private static readonly Lazy<LocaleString> _protoFluxLocale = new(() => Mod.GetLocaleString("WikiHyperlink.ProtoFlux"));

        public override int Priority => HarmonyLib.Priority.HigherThanNormal;
        private static LocaleString ComponentLocale => _componentLocale.Value;
        private static LocaleString ProtoFluxLocale => _protoFluxLocale.Value;

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

                wikiPage = $"ProtoFlux:{nodeName.Replace(' ', '_')}";
            }
            else
            {
                reason = ComponentLocale;
                var workerName = worker.WorkerType.Name;

                // Don't need to remove the `1 on generics - they redirect
                wikiPage = $"Component:{workerName}";
            }

            var hyperlink = slot.AttachComponent<Hyperlink>();
            hyperlink.URL.Value = new Uri($"https://wiki.resonite.com/{wikiPage}");
            hyperlink.Reason.AssignLocaleString(reason);
        }

        private static void Postfix(ProtoFluxNodeVisual __instance, ProtoFluxNode node)
        {
            if (node.SupressHeaderAndFooter && node.NodeName.Contains("Relay", StringComparison.OrdinalIgnoreCase))
                return;

            var ui = new UIBuilder(__instance.LocalUIBuilder.Canvas);

            var buttonArea = ui.Panel();
            ui.IgnoreLayout();
            buttonArea.AnchorMin.Value = new(1, 0);
            buttonArea.AnchorMax.Value = new(1, 0);
            buttonArea.OffsetMin.Value = new(-12, 2);
            buttonArea.OffsetMax.Value = new(-2, 12);

            var button = ui.Image(OfficialAssets.Graphics.Badges.Mentor);
            button.Slot.AttachComponent<Button>().WithTooltip(ProtoFluxLocale);

            AddHyperlink(button.Slot, node);

            ConfigSection.ProtoFlux.DriveFromVariable(button.Slot.ActiveSelf_Field);
        }
    }
}