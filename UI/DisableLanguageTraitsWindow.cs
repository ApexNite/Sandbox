using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using Sandbox.Toolkit.Graphics;
using UnityEngine;

namespace Sandbox.UI {
    internal class DisableLanguageTraitsWindow : AutoLayoutWindow<DisableLanguageTraitsWindow> {
        private static DisableLanguageTraitsWindow _instance;
        private AutoGridLayoutGroup _grid;
        private List<string> _loadedTraits;
        private AutoVertLayoutGroup _vert;

        [HarmonyPatch(typeof(LanguageManager), nameof(LanguageManager.addRandomTraitFromBiomeToLanguage))]
        [HarmonyPrefix]
        public static bool addRandomTraitFromBiomeToLanguage(Language pLanguage, WorldTile pTile) {
            BiomeAsset biomeAsset = pTile.Type.biome_asset;
            pLanguage.addRandomTraitFromBiome(pTile,
                biomeAsset?.spawn_trait_language?.Where(LanguageTraitEnabled).ToList(), AssetManager.language_traits);

            return false;
        }

        [HarmonyPatch(typeof(Language), nameof(Language.default_traits), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool get_default_traits(ref Language __instance, ref List<string> __result) {
            __result = __instance.getActorAsset().default_language_traits?.Where(LanguageTraitEnabled).ToList();

            return false;
        }

        [HarmonyPatch(typeof(BehFinishReading), nameof(BehFinishReading.tryToConvertActorToBookLanguage))]
        [HarmonyPrefix]
        public static bool tryToConvertActorToBookLanguage(Actor pActor, Book pBook) {
            LanguageTrait trait = pBook.getBookTraitLanguage();

            return trait != null && LanguageTraitEnabled(trait.id);
        }

        [HarmonyPatch(typeof(LanguageManager), nameof(LanguageManager.newLanguage))]
        [HarmonyPrefix]
        public static bool newLanguage(Actor pActor, bool pAddDefaultTraits, ref LanguageManager __instance,
            ref Language __result) {
            World.world.game_stats.data.languagesCreated += 1L;
            World.world.map_stats.languagesCreated += 1L;
            Language newLanguage = __instance.newObject();
            newLanguage.newLanguage(pActor, pAddDefaultTraits);
            AddRandomEnabledTrait(newLanguage);
            __instance.addRandomTraitFromBiomeToLanguage(newLanguage, pActor.current_tile);
            __result = newLanguage;

            return false;
        }

        private static void AddRandomEnabledTrait(Language language) {
            int min = 1;
            int max = 3;
            if (WorldLawLibrary.world_law_glitched_noosphere.isEnabled()) {
                min = 3;
                max = 6;
            }

            int amount = Randy.randomInt(min, max);
            int fails = 0;
            for (int i = 0; i < amount; i++) {
                LanguageTrait trait = AssetManager.language_traits.getRandomSpawnTrait();

                if (!LanguageTraitEnabled(trait.id) && fails < 5) {
                    i--;
                    fails++;
                    continue;
                }

                if (trait.isAvailable() && LanguageTraitEnabled(trait.id)) {
                    language.addTrait(trait, true);
                }
            }
        }

        private static bool LanguageTraitEnabled(string traitId) {
            string id = $"{traitId}_language_trait_toggle";

            if (!PlayerConfig.dict.ContainsKey(id)) {
                _instance.UpdateButtons();
            }

            return PlayerConfig.dict.ContainsKey(id) && PlayerConfig.optionBoolEnabled(id);
        }

        public override void OnNormalEnable() {
            UpdateButtons();
        }

        protected override void Init() {
            _grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));
            _vert = this.BeginVertGroup();
            _loadedTraits = new List<string>();

            new ButtonBuilder("remove_disabled_language_traits", ButtonStyle.SpecialRed)
                .SetScale(new Vector2(210f, 23f))
                .SetText("remove_disabled_language_traits")
                .SetAction(() => {
                    foreach (Language language in World.world.languages) {
                        foreach (LanguageTrait languageTrait in language.getTraits().ToList()) {
                            if (!LanguageTraitEnabled(languageTrait.id)) {
                                language.removeTrait(languageTrait);
                            }
                        }
                    }
                })
                .Build(out PowerButton removeDisabledButton);

            _vert.AddChild(_grid.gameObject);
            _vert.AddChild(removeDisabledButton.gameObject);

            UpdateButtons();

            _instance = this;

            Harmony.CreateAndPatchAll(typeof(DisableLanguageTraitsWindow));
        }

        private void UpdateButtons() {
            foreach (LanguageTrait languageTrait in AssetManager.language_traits.list) {
                string id = $"{languageTrait.id}_language_trait_toggle";

                if (_loadedTraits.Contains(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                    LocalizedTextManager.add($"{id}_description", $"{id}_description");
                }

                bool firstRun = PlayerConfig.instance.data.list.All(data => data.name != id);
                Sprite sprite = SpriteTextureLoader.getSprite(languageTrait.path_icon);

                if (sprite == null) {
                    sprite = SpriteTextureLoader.getSprite("ui/icons/iconQuestionMark");
                }

                AssetManager.powers.add(new GodPower { id = id, name = id, toggle_name = id });
                _grid.AddChild(PowerButtonCreator.CreateToggleButton(id, sprite).gameObject);
                _loadedTraits.Add(id);

                if (firstRun) {
                    PlayerConfig.setOptionBool(id, true);
                }
            }
        }
    }
}