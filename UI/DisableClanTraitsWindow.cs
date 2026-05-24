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
    internal class DisableClanTraitsWindow : AutoLayoutWindow<DisableClanTraitsWindow> {
        private static DisableClanTraitsWindow _instance;
        private AutoGridLayoutGroup _grid;
        private List<string> _loadedTraits;
        private AutoVertLayoutGroup _vert;

        [HarmonyPatch(typeof(ClanManager), nameof(ClanManager.addRandomTraitFromBiomeToClan))]
        [HarmonyPrefix]
        public static bool addRandomTraitFromBiomeToClan(Clan pClan, WorldTile pTile) {
            BiomeAsset biomeAsset = pTile.Type.biome_asset;
            pClan.addRandomTraitFromBiome(pTile,
                biomeAsset?.spawn_trait_clan?.Where(ClanTraitEnabled).ToList(), AssetManager.clan_traits);

            return false;
        }

        [HarmonyPatch(typeof(Clan), nameof(Clan.default_traits), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool get_default_traits(ref Clan __instance, ref List<string> __result) {
            __result = __instance.getActorAsset().default_clan_traits.Where(ClanTraitEnabled).ToList();

            return false;
        }

        [HarmonyPatch(typeof(ClanManager), nameof(ClanManager.newClan))]
        [HarmonyPrefix]
        public static bool newClan(Actor pFounder, bool pAddDefaultTraits, ref ClanManager __instance,
            ref Clan __result) {
            World.world.game_stats.data.clansCreated += 1L;
            World.world.map_stats.clansCreated += 1L;
            Clan newClan = __instance.newObject();
            newClan.newClan(pFounder, pAddDefaultTraits);
            AddRandomEnabledTrait(newClan);
            pFounder.setClan(newClan);
            if (pFounder.isKing()) {
                pFounder.kingdom.trySetRoyalClan();
            }

            __instance.convertFamilyToClan(pFounder, newClan);
            __instance.addRandomTraitFromBiomeToClan(newClan, pFounder.current_tile);
            __result = newClan;

            return false;
        }

        private static void AddRandomEnabledTrait(Clan clan) {
            int min = 1;
            int max = 3;
            if (WorldLawLibrary.world_law_glitched_noosphere.isEnabled()) {
                min = 3;
                max = 6;
            }

            int amount = Randy.randomInt(min, max);
            int fails = 0;
            for (int i = 0; i < amount; i++) {
                ClanTrait trait = AssetManager.clan_traits.getRandomSpawnTrait();

                if (!ClanTraitEnabled(trait.id) && fails < 5) {
                    i--;
                    fails++;
                    continue;
                }

                if (trait.isAvailable() && ClanTraitEnabled(trait.id)) {
                    clan.addTrait(trait, true);
                }
            }
        }

        private static bool ClanTraitEnabled(string traitId) {
            string id = $"{traitId}_clan_trait_toggle";

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

            new ButtonBuilder("remove_disabled_clan_traits", ButtonStyle.SpecialRed)
                .SetScale(new Vector2(210f, 23f))
                .SetText("remove_disabled_clan_traits")
                .SetAction(() => {
                    foreach (Clan clan in World.world.clans) {
                        foreach (ClanTrait clanTrait in clan.getTraits().ToList()) {
                            if (!ClanTraitEnabled(clanTrait.id)) {
                                clan.removeTrait(clanTrait);
                            }
                        }
                    }
                })
                .Build(out PowerButton removeDisabledButton);

            _vert.AddChild(_grid.gameObject);
            _vert.AddChild(removeDisabledButton.gameObject);

            UpdateButtons();

            _instance = this;

            PlotAsset plotAsset = AssetManager.plots_library.get("clan_ascension");
            plotAsset.action += actor => {
                Clan clan = actor.clan;

                if (clan != null && clan.hasTrait("mark_of_becoming") && !ClanTraitEnabled("mark_of_becoming")) {
                    clan.removeTrait("mark_of_becoming");
                }

                return true;
            };

            Harmony.CreateAndPatchAll(typeof(DisableClanTraitsWindow));
        }

        private void UpdateButtons() {
            foreach (ClanTrait clanTrait in AssetManager.clan_traits.list) {
                string id = $"{clanTrait.id}_clan_trait_toggle";

                if (_loadedTraits.Contains(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                    LocalizedTextManager.add($"{id}_description", $"{id}_description");
                }

                bool firstRun = PlayerConfig.instance.data.list.All(data => data.name != id);
                Sprite sprite = SpriteTextureLoader.getSprite(clanTrait.path_icon);

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