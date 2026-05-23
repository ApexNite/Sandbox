using System.Collections.Generic;
using HarmonyLib;
using Sandbox.UI;
using UnityEngine;

namespace Sandbox.Features {
    [HarmonyPatch]
    internal class MagnetPlus : Magnet {
        private static MagnetFilterMode _filterMode = MagnetFilterMode.None;
        private static MagnetPlus _instance;

        private enum MagnetFilterMode {
            None,
            Culture,
            Language,
            Religion,
            Subspecies
        }

        [HarmonyPatch(typeof(QuantumSpriteLibrary), nameof(QuantumSpriteLibrary.drawMagnetUnits))]
        [HarmonyPostfix]
        public static void drawMagnetUnits(QuantumSpriteAsset pAsset) {
            if (!_instance.hasUnits()) {
                return;
            }

            List<Actor> tList = _instance.magnet_units;

            foreach (Actor tUnit in tList) {
                if (tUnit.isRekt()) {
                    continue;
                }

                QuantumSprite quantumSprite =
                    QuantumSpriteLibrary.drawQuantumSprite(pAsset, tUnit.current_position, null, tUnit.kingdom);
                quantumSprite.setScale(tUnit.current_scale.y);
                quantumSprite.transform.rotation = Quaternion.Euler(0f, 0f, World.world.magnet.moving_angle);
                quantumSprite.setSprite(tUnit.getSpriteToRender());
            }
        }

        public static void Init() {
            _instance = new MagnetPlus();
            GodPower magnetPlusPower = new GodPower {
                id = "magnet_plus",
                name = "magnet_plus",
                show_tool_sizes = true,
                hold_action = true,
                highlight = true,
                sound_drawing = "event:/SFX/POWERS/DivineMagnet",
                unselect_when_window = true,
            };
            magnetPlusPower.click_brush_action += UseMagnetPlus;
            magnetPlusPower.click_brush_action += AssetManager.powers.flashBrushPixelsDuringClick;
            magnetPlusPower.click_brush_action += AssetManager.powers.fmodDrawingSound;

            AssetManager.powers.add(magnetPlusPower);
            AssetManager.powers.clone("culture_magnet", "magnet_plus");
            AssetManager.powers.clone("language_magnet", "magnet_plus");
            AssetManager.powers.clone("religion_magnet", "magnet_plus");
            AssetManager.powers.clone("subspecies_magnet", "magnet_plus");

            MagnetPlusSelector.CreateWindow("magnet_plus_editor", "magnet_plus_editor");
            CultureMagnetSelector.CreateWindow("culture_magnet", "culture_magnet");
            LanguageMagnetSelector.CreateWindow("language_magnet", "language_magnet");
            ReligionMagnetSelector.CreateWindow("religion_magnet", "religion_magnet");
            SubspeciesMagnetSelector.CreateWindow("subspecies_magnet", "subspecies_magnet");

            Harmony.CreateAndPatchAll(typeof(MagnetPlus));
        }

        [HarmonyPatch(typeof(Magnet), nameof(magnetAction))]
        [HarmonyPrefix]
        public static bool magnetActionPatch(bool pFromUpdate, WorldTile pTile = null) {
            if (pFromUpdate && _instance != null) {
                _instance.MagnetAction(true, pTile);
            }

            return true;
        }

        private static bool UseMagnetPlus(WorldTile worldTile, string powerId) {
            switch (powerId) {
                case "culture_magnet":
                    _filterMode = MagnetFilterMode.Culture;

                    break;
                case "language_magnet":
                    _filterMode = MagnetFilterMode.Language;

                    break;
                case "religion_magnet":
                    _filterMode = MagnetFilterMode.Religion;

                    break;
                case "subspecies_magnet":
                    _filterMode = MagnetFilterMode.Subspecies;

                    break;
                default:
                    _filterMode = MagnetFilterMode.None;

                    break;
            }

            _instance.MagnetAction(false, worldTile);

            return true;
        }

        private void MagnetAction(bool pFromUpdate, WorldTile pTile = null) {
            if (ScrollWindow.isWindowActive()) {
                dropPickedUnits();

                return;
            }

            if (pFromUpdate && _magnet_state != 1 && _magnet_state != 3) {
                return;
            }

            if (pTile != null) {
                _magnet_last_pos = pTile;
            }

            _magnet_throw.trackMouseMovement(_magnet_state);
            updatePickedUnits();

            if (pTile != null) {
                World.world.flash_effects.flashPixel(pTile, 10);
            }

            switch (_magnet_state) {
                case 0:
                    if (Input.GetMouseButton(0)) {
                        _magnet_state = 1;
                        _magnet_throw.initializeMouseTracking();
                    }

                    break;
                case 1:
                    if (!pFromUpdate) {
                        PickupUnits(pTile);
                    }

                    if (Input.GetMouseButtonUp(0)) {
                        _magnet_state = 2;
                        dropPickedUnits();
                    }

                    break;
                case 2:
                    if (!pFromUpdate && Input.GetMouseButton(0)) {
                        dropPickedUnits();
                        _magnet_state = 0;
                    }

                    break;
                default:
                    return;
            }
        }

        private static bool PassesFilter(Actor actor) {
            if (actor.isRekt()) {
                return false;
            }

            switch (_filterMode) {
                case MagnetFilterMode.Culture:
                {
                    Culture target = CultureMagnetSelector.LastSelectedCulture;
                    Culture current = actor.culture;

                    return target != null && current != null && current == target;
                }
                case MagnetFilterMode.Religion:
                {
                    Religion target = ReligionMagnetSelector.LastSelectedReligion;
                    Religion current = actor.religion;

                    return target != null && current != null && current == target;
                }
                case MagnetFilterMode.Language:
                {
                    Language target = LanguageMagnetSelector.LastSelectedLanguage;
                    Language current = actor.language;

                    return target != null && current != null && current == target;
                }
                case MagnetFilterMode.Subspecies:
                {
                    Subspecies target = SubspeciesMagnetSelector.LastSelectedSubspecies;
                    Subspecies current = actor.subspecies;

                    return target != null && current != null && current == target;
                }
                case MagnetFilterMode.None:
                default:
                    return true;
            }
        }

        private void PickupUnits(WorldTile worldTile) {
            BrushPixelData[] brushPixel = Config.current_brush_data.pos;

            for (int i = 0; i < brushPixel.Length; i++) {
                WorldTile tile = World.world.GetTile(brushPixel[i].x + worldTile.x, brushPixel[i].y + worldTile.y);

                if (tile != null && tile.hasUnits()) {
                    tile.doUnits(delegate(Actor actor) {
                        string id = $"{actor.asset.id}_magnet_toggle".Underscore();

                        if (!actor.asset.can_be_moved_by_powers) {
                            return;
                        }

                        if (actor.isInsideSomething()) {
                            return;
                        }

                        if (!PassesFilter(actor)) {
                            return;
                        }

                        if (PlayerConfig.dict.ContainsKey(id)
                            && !PlayerConfig.optionBoolEnabled(id)
                            && !(actor.asset.unit_zombie && PlayerConfig.optionBoolEnabled("zombie_magnet_toggle"))) {
                            return;
                        }

                        if (!_magnet_units.Add(actor)) {
                            return;
                        }

                        actor.cancelAllBeh();
                        magnet_units.Add(actor);
                        actor.is_in_magnet = true;
                        _picked_up_multiplier = 2f;
                    });
                }
            }

            _has_units = _magnet_units.Count > 0;
        }
    }
}