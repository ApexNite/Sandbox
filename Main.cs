using NeoModLoader.api;
using Sandbox.Features;
using Sandbox.UI;

namespace Sandbox {
    public class Main : BasicMod<Main> {
        protected override void OnModLoad() {
            Config.preload_windows = true;

            MagnetPlus.Init();
            BuildingConstructor.Init();
            ForceUnitCity.Init();
            ForceUnitCulture.Init();
            ForceUnitLanguage.Init();
            ForceUnitReligion.Init();
            ForceUnitPlot.Init();
            SettleCity.Init();
            MakeUnitKing.Init();
            MakeUnitLeader.Init();
            DisableClanTraitsWindow.CreateWindow("disable_clan_traits", "disable_clan_traits");
            DisableCultureTraitsWindow.CreateWindow("disable_culture_traits", "disable_culture_traits");
            DisableLanguageTraitsWindow.CreateWindow("disable_language_traits", "disable_language_traits");
            DisableReligionTraitsWindow.CreateWindow("disable_religion_traits", "disable_religion_traits");
            ForcedGeneEdit.Init();
            SandboxTab.Init();
        }
    }
}