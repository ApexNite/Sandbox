using System.Collections.Generic;
using NeoModLoader.General.UI.Tab;
using Sandbox.Toolkit.Graphics;
using UnityEngine;

namespace Sandbox.UI {
    internal static class SandboxTab {
        private const string BuildingConstructor = "building_constructor";
        private const string KingdomManipulation = "kingdom_manipulation";
        private const string MagnetPlus = "magnet_plus";
        private const string TraitDisablers = "trait_disablers";
        private const string UnitManipulation = "unit_manipulation";
        private static PowerButton _buildingConstructorButton;
        private static PowerButton _directSettleCityButton;
        private static PowerButton _disableClanTraitsButton;
        private static PowerButton _disableCultureTraitsButton;
        private static PowerButton _disableLanguageTraitsButton;
        private static PowerButton _disableReligionTraitsButton;
        private static PowerButton _forceCityKingdomButton;
        private static PowerButton _forcePlotButton;
        private static PowerButton _forceUnitCity;
        private static PowerButton _forceUnitCulture;
        private static PowerButton _forceUnitLanguage;
        private static PowerButton _forceUnitProfessionButton;
        private static PowerButton _forceUnitReligion;
        private static PowerButton _magnetPlusButton;
        private static PowerButton _magnetPlusEditorButton;
        private static PowerButton _pToPSettleCityButton;
        private static PowersTab _tab;

        public static void Init() {
            _tab = TabManager.CreateTab("sandbox", "tab_sandbox", "tab_sandbox_description",
                SpriteTextureLoader.getSprite("ui/icons/tab_icon"));
            CreateButtons();

            _tab._children = 26;
            _tab.SetLayout(new List<string> {
                UnitManipulation,
                KingdomManipulation,
                TraitDisablers,
                MagnetPlus,
                BuildingConstructor
            });
            _tab.AddPowerButton(UnitManipulation, _forceUnitCity);
            _tab.AddPowerButton(UnitManipulation, _forceUnitCulture);
            _tab.AddPowerButton(UnitManipulation, _forceUnitLanguage);
            _tab.AddPowerButton(UnitManipulation, _forceUnitReligion);
            _tab.AddPowerButton(UnitManipulation, _forceUnitProfessionButton);
            _tab.AddPowerButton(KingdomManipulation, _forceCityKingdomButton);
            _tab.AddPowerButton(KingdomManipulation, _forcePlotButton);
            _tab.AddPowerButton(KingdomManipulation, _pToPSettleCityButton);
            _tab.AddPowerButton(KingdomManipulation, _directSettleCityButton);
            _tab.AddPowerButton(TraitDisablers, _disableClanTraitsButton);
            _tab.AddPowerButton(TraitDisablers, _disableCultureTraitsButton);
            _tab.AddPowerButton(TraitDisablers, _disableLanguageTraitsButton);
            _tab.AddPowerButton(TraitDisablers, _disableReligionTraitsButton);
            _tab.AddPowerButton(MagnetPlus, _magnetPlusEditorButton);
            _tab.AddPowerButton(MagnetPlus, _magnetPlusButton);
            _tab.AddPowerButton(BuildingConstructor, _buildingConstructorButton);
            _tab.UpdateLayout();

            for (int i = 0; i < _tab.gameObject.transform.childCount; i++) {
                GameObject child = _tab.gameObject.transform.GetChild(i).gameObject;

                if (child.name == "_line(Clone)") {
                    child.transform.localPosition -= new Vector3(0, 72f);
                }
            }
        }

        private static void CreateButtons() {
            new ButtonBuilder("magnet_plus_editor", ButtonStyle.SpecialRedBorder).SetIcon("ui/icons/magnet_plus_icon")
                .SetWindowId("magnet_plus_editor")
                .Build(out _magnetPlusEditorButton)
                .Next("magnet_plus", ButtonStyle.Small)
                .SetIcon("ui/icons/magnet_plus_icon")
                .SetGodPower("magnet_plus")
                .Build(out _magnetPlusButton)
                .Next("building_constructor", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/building_constructor_icon")
                .SetWindowId("building_constructor")
                .Build(out _buildingConstructorButton)
                .Next("force_unit_city", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/force_unit_city_icon")
                .SetWindowId("force_unit_city")
                .Build(out _forceUnitCity)
                .Next("force_unit_culture", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/force_culture_icon")
                .SetWindowId("force_unit_culture")
                .Build(out _forceUnitCulture)
                .Next("force_unit_language", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/force_language_icon")
                .SetWindowId("force_unit_language")
                .Build(out _forceUnitLanguage)
                .Next("force_unit_religion", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/force_religion_icon")
                .SetWindowId("force_unit_religion")
                .Build(out _forceUnitReligion)
                .Next("force_plot", ButtonStyle.Small)
                .SetIcon("ui/icons/force_plot_icon")
                .SetGodPower("force_plot")
                .Build(out _forcePlotButton)
                .Next("ptop_settle_city", ButtonStyle.Small)
                .SetIcon("ui/icons/ptop_settle_city_icon")
                .SetGodPower("ptop_settle_city")
                .Build(out _pToPSettleCityButton)
                .Next("direct_settle_city", ButtonStyle.Small)
                .SetIcon("ui/icons/direct_settle_city_icon")
                .SetGodPower("direct_settle_city")
                .Build(out _directSettleCityButton)
                .Next("disable_clan_traits", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/no_clan_traits_icon")
                .SetWindowId("disable_clan_traits")
                .Build(out _disableClanTraitsButton)
                .Next("disable_culture_traits", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/no_culture_traits_icon")
                .SetWindowId("disable_culture_traits")
                .Build(out _disableCultureTraitsButton)
                .Next("disable_language_traits", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/no_language_traits_icon")
                .SetWindowId("disable_language_traits")
                .Build(out _disableLanguageTraitsButton)
                .Next("disable_religion_traits", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/no_religion_traits_icon")
                .SetWindowId("disable_religion_traits")
                .Build(out _disableReligionTraitsButton)
                .Next("force_city_kingdom", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/iconQuestionMark")
                .SetWindowId("force_city_kingdom")
                .Build(out _forceCityKingdomButton)
                .Next("force_unit_profession", ButtonStyle.SpecialRedBorder)
                .SetIcon("ui/icons/iconQuestionMark")
                .SetWindowId("force_unit_profession")
                .Build(out _forceUnitProfessionButton);
        }
    }
}