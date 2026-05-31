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
    internal class DisableCultureTraitsWindow : AutoLayoutWindow<DisableCultureTraitsWindow> {
        private static DisableCultureTraitsWindow _instance;
        private AutoGridLayoutGroup _grid;
        private List<string> _loadedTraits;
        private AutoVertLayoutGroup _vert;

        [HarmonyPatch(typeof(CultureManager), nameof(CultureManager.addRandomTraitFromBiomeToCulture))]
        [HarmonyPrefix]
        public static bool addRandomTraitFromBiomeToCulture(Culture pCulture, WorldTile pTile) {
            BiomeAsset biomeAsset = pTile.Type.biome_asset;
            pCulture.addRandomTraitFromBiome(pTile,
                biomeAsset?.spawn_trait_culture?.Where(CultureTraitEnabled).ToList(), AssetManager.culture_traits);

            return false;
        }

        [HarmonyPatch(typeof(Culture), nameof(Culture.default_traits), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool get_default_traits(ref Culture __instance, ref List<string> __result) {
            __result = __instance.getActorAsset().default_culture_traits?.Where(CultureTraitEnabled).ToList();

            return false;
        }

        [HarmonyPatch(typeof(BehFinishReading), nameof(BehFinishReading.tryToGetMetaTraitFromBookCulture))]
        [HarmonyPrefix]
        public static bool tryToGetMetaTraitFromBookCulture(Actor pActor, Book pBook) {
            CultureTrait trait = pBook.getBookTraitCulture();

            return trait != null && CultureTraitEnabled(trait.id);
        }

        [HarmonyPatch(typeof(CultureManager), nameof(CultureManager.newCulture))]
        [HarmonyPrefix]
        public static bool newCulture(Actor pFounder, bool pAddDefaultTraits, ref CultureManager __instance,
            ref Culture __result) {
            World.world.game_stats.data.culturesCreated += 1L;
            World.world.map_stats.culturesCreated += 1L;
            Culture newCulture = __instance.newObject();
            newCulture.createCulture(pFounder, pAddDefaultTraits);
            AddRandomEnabledTrait(newCulture);
            __instance.addRandomTraitFromBiomeToCulture(newCulture, pFounder.current_tile);
            __result = newCulture;

            return false;
        }

        private static void AddRandomEnabledTrait(Culture culture) {
            int min = 1;
            int max = 3;
            if (WorldLawLibrary.world_law_glitched_noosphere.isEnabled()) {
                min = 3;
                max = 6;
            }

            int amount = Randy.randomInt(min, max);
            int fails = 0;
            for (int i = 0; i < amount; i++) {
                CultureTrait trait = AssetManager.culture_traits.getRandomSpawnTrait();

                if (!CultureTraitEnabled(trait.id) && fails < 5) {
                    i--;
                    fails++;
                    continue;
                }

                if (trait.isAvailable() && CultureTraitEnabled(trait.id)) {
                    culture.addTrait(trait, true);
                }
            }
        }

        private static bool CultureTraitEnabled(string traitId) {
            string id = $"{traitId}_culture_trait_toggle";

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

            new ButtonBuilder("remove_disabled_culture_traits", ButtonStyle.SpecialRed)
                .SetScale(new Vector2(210f, 23f))
                .SetText("remove_disabled_culture_traits")
                .SetAction(() => {
                    foreach (Culture culture in World.world.cultures) {
                        foreach (CultureTrait cultureTrait in culture.getTraits().ToList()) {
                            if (!CultureTraitEnabled(cultureTrait.id)) {
                                culture.removeTrait(cultureTrait);
                            }
                        }
                    }
                })
                .Build(out PowerButton removeDisabledButton);

            _vert.AddChild(_grid.gameObject);
            _vert.AddChild(removeDisabledButton.gameObject);

            UpdateButtons();

            _instance = this;

            Harmony.CreateAndPatchAll(typeof(DisableCultureTraitsWindow));
        }

        private void UpdateButtons() {
            foreach (CultureTrait cultureTrait in AssetManager.culture_traits.list) {
                string id = $"{cultureTrait.id}_culture_trait_toggle";

                if (_loadedTraits.Contains(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                    LocalizedTextManager.add($"{id}_description", $"{id}_description");
                }

                bool firstRun = PlayerConfig.instance.data.list.All(data => data.name != id);
                Sprite sprite = SpriteTextureLoader.getSprite(cultureTrait.path_icon);

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