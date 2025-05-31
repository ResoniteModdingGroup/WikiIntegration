using ComponentSelectorAdditions.Events;
using Elements.Core;
using FrooxEngine.UIX;
using FrooxEngine;
using MonkeyLoader.Resonite;
using System;
using MonkeyLoader.Resonite.UI;
using MonkeyLoader.Resonite.Configuration;
using ComponentSelectorAdditions;
using System.Collections.Generic;

namespace WikiIntegration
{
    internal sealed class CategoryWikiArticleButtons : ResoniteEventHandlerMonkey<CategoryWikiArticleButtons, PostProcessButtonsEvent>
    {
        private const string ProtoFluxPath = "/ProtoFlux/Runtimes/Execution/Nodes";

        private static readonly Lazy<LocaleString> _componentLocale = new(() => Mod.GetLocaleString("WikiHyperlink.ComponentCategory"));
        private static readonly Lazy<LocaleString> _protoFluxLocale = new(() => Mod.GetLocaleString("WikiHyperlink.ProtoFluxCategory"));

        public override IEnumerable<string> Authors { get; } = ["Banane9"];

        public override int Priority => HarmonyLib.Priority.Normal;

        private static LocaleString ComponentLocale => _componentLocale.Value;
        private static LocaleString ProtoFluxLocale => _protoFluxLocale.Value;

        protected override void Handle(PostProcessButtonsEvent eventData)
        {
            if (!Enabled)
                return;

            var isProtoFlux = eventData.Path.Path.StartsWith(ProtoFluxPath);

            var builder = eventData.UI;

            foreach (var button in eventData.CategoryButtons)
            {
                var path = button.Slot.GetComponent<ButtonRelay<string>>().Argument.Value;

                // more generalized check instead of: path.EndsWith("Favorites") || path.EndsWith("Recents"))
                // presuming that all non-user excluded categories are ones added by mods
                // which won't have a wiki article to link to anyways
                if (SearchConfig.Instance.HasExcludedCategory(path, out var isUserCategory) && !isUserCategory.Value)
                    continue;

                builder.NestInto(button.Slot.Parent);

                var height = button.Slot.Parent.GetComponent<LayoutElement>().MinHeight;
                builder.Style.MinWidth = height;

                string wikiPage;
                LocaleString reason;
                ConfigKeySessionShare<bool> visibilityShare;

                if (isProtoFlux)
                {
                    reason = ProtoFluxLocale;
                    wikiPage = $"ProtoFlux:{path.Replace(ProtoFluxPath, "").TrimStart('/').Replace('/', ':')}";
                    visibilityShare = WikiButtonConfig.Instance.ProtoFluxCategories;
                }
                else
                {
                    reason = ComponentLocale;
                    wikiPage = $"Components:{path.TrimStart('/').Replace('/', ':')}";
                    visibilityShare = WikiButtonConfig.Instance.ComponentCategories;
                }

                var wikiButton = builder.Button(OfficialAssets.Graphics.Badges.Mentor).WithTooltip(reason);
                visibilityShare.DriveFromVariable(wikiButton.Slot.ActiveSelf_Field);
                wikiButton.Slot.OrderOffset = -1;

                var hyperlink = wikiButton.Slot.AttachComponent<Hyperlink>();
                hyperlink.URL.Value = new Uri($"https://wiki.resonite.com/Category:{wikiPage}");
                hyperlink.Reason.AssignLocaleString(reason);
            }
        }
    }
}