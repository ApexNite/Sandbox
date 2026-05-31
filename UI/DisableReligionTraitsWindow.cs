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
    internal class DisableReligionTraitsWindow : AutoLayoutWindow<DisableReligionTraitsWindow> {
        private static DisableReligionTraitsWindow _instance;
        private AutoGridLayoutGroup _grid;
        private List<string> _loadedTraits;
        private AutoVertLayoutGroup _vert;

        [HarmonyPatch(typeof(ReligionManager), nameof(ReligionManager.addRandomTraitFromBiomeToReligion))]
        [HarmonyPrefix]
        public static bool addRandomTraitFromBiomeToReligion(Religion pReligion, WorldTile pTile) {
            BiomeAsset biomeAsset = pTile.Type.biome_asset;
            pReligion.addRandomTraitFromBiome(pTile,
                biomeAsset?.spawn_trait_religion?.Where(ReligionTraitEnabled).ToList(), AssetManager.religion_traits);

            return false;
        }

        [HarmonyPatch(typeof(Religion), nameof(Religion.default_traits), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool get_default_traits(ref Religion __instance, ref List<string> __result) {
            __result = __instance.getActorAsset().default_religion_traits?.Where(ReligionTraitEnabled).ToList();

            return false;
        }

        [HarmonyPatch(typeof(BehFinishReading), nameof(BehFinishReading.tryToConvertActorToBookReligion))]
        [HarmonyPrefix]
        public static bool tryToConvertActorToBookReligion(Actor pActor, Book pBook) {
            ReligionTrait trait = pBook.getBookTraitReligion();

            return trait != null && ReligionTraitEnabled(trait.id);
        }

        [HarmonyPatch(typeof(ReligionManager), nameof(ReligionManager.newReligion))]
        [HarmonyPrefix]
        public static bool newReligion(Actor pFounder, bool pAddDefaultTraits, ref ReligionManager __instance,
            ref Religion __result) {
            World.world.game_stats.data.religionsCreated += 1L;
            World.world.map_stats.religionsCreated += 1L;
            Religion newReligion = __instance.newObject();
            newReligion.newReligion(pFounder, pFounder.current_tile, pAddDefaultTraits);
            AddRandomEnabledTrait(newReligion);
            __instance.addRandomTraitFromBiomeToReligion(newReligion, pFounder.current_tile);
            __result = newReligion;

            return false;
        }

        private static void AddRandomEnabledTrait(Religion religion) {
            int min = 1;
            int max = 3;
            if (WorldLawLibrary.world_law_glitched_noosphere.isEnabled()) {
                min = 3;
                max = 6;
            }

            int amount = Randy.randomInt(min, max);
            int fails = 0;
            for (int i = 0; i < amount; i++) {
                ReligionTrait trait = AssetManager.religion_traits.getRandomSpawnTrait();

                if (!ReligionTraitEnabled(trait.id) && fails < 5) {
                    i--;
                    fails++;
                    continue;
                }

                if (trait.isAvailable() && ReligionTraitEnabled(trait.id)) {
                    religion.addTrait(trait, true);
                }
            }
        }

        private static bool ReligionTraitEnabled(string traitId) {
            string id = $"{traitId}_religion_trait_toggle";

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

            new ButtonBuilder("remove_disabled_religion_traits", ButtonStyle.SpecialRed)
                .SetScale(new Vector2(210f, 23f))
                .SetText("remove_disabled_religion_traits")
                .SetAction(() => {
                    foreach (Religion religion in World.world.religions) {
                        foreach (ReligionTrait religionTrait in religion.getTraits().ToList()) {
                            if (!ReligionTraitEnabled(religionTrait.id)) {
                                religion.removeTrait(religionTrait);
                            }
                        }
                    }
                })
                .Build(out PowerButton removeDisabledButton);

            _vert.AddChild(_grid.gameObject);
            _vert.AddChild(removeDisabledButton.gameObject);

            UpdateButtons();

            _instance = this;

            Harmony.CreateAndPatchAll(typeof(DisableReligionTraitsWindow));
        }

        private void UpdateButtons() {
            foreach (ReligionTrait religionTrait in AssetManager.religion_traits.list) {
                string id = $"{religionTrait.id}_religion_trait_toggle";

                if (_loadedTraits.Contains(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                    LocalizedTextManager.add($"{id}_description", $"{id}_description");
                }

                bool firstRun = PlayerConfig.instance.data.list.All(data => data.name != id);
                Sprite sprite = SpriteTextureLoader.getSprite(religionTrait.path_icon);

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