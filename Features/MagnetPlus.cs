using System.Collections.Generic;
using HarmonyLib;
using Sandbox.UI;
using UnityEngine;

namespace Sandbox.Features {
    [HarmonyPatch]
    internal class MagnetPlus : Magnet {
        private static GodPower _godPower;
        private static MagnetPlus _instance;

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
            _godPower = new GodPower {
                id = "magnet_plus",
                name = "magnet_plus",
                show_tool_sizes = true,
                hold_action = true,
                highlight = true,
                sound_drawing = "event:/SFX/POWERS/DivineMagnet",
                unselect_when_window = true
            };

            _godPower.click_brush_action += UseMagnetPlus;
            _godPower.click_brush_action += AssetManager.powers.flashBrushPixelsDuringClick;
            _godPower.click_brush_action += AssetManager.powers.fmodDrawingSound;

            AssetManager.powers.add(_godPower);
            MagnetPlusEditor.CreateWindow("magnet_plus_editor", "magnet_plus_editor");
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

        private void PickupUnits(WorldTile worldTile) {
            BrushPixelData[] tBrushPixels = Config.current_brush_data.pos;

            for (int i = 0; i < tBrushPixels.Length; i++) {
                WorldTile tTile = World.world.GetTile(tBrushPixels[i].x + worldTile.x, tBrushPixels[i].y + worldTile.y);

                if (tTile != null && tTile.hasUnits()) {
                    tTile.doUnits(delegate(Actor tActor) {
                        if (!tActor.asset.can_be_moved_by_powers) {
                            return;
                        }

                        if (tActor.isInsideSomething()) {
                            return;
                        }

                        if (!PlayerConfig.optionBoolEnabled(tActor.asset.id + "_magnet_toggle")
                            && !(tActor.asset.unit_zombie && PlayerConfig.optionBoolEnabled("zombie_magnet_toggle"))) {
                            return;
                        }

                        if (!_magnet_units.Add(tActor)) {
                            return;
                        }

                        tActor.cancelAllBeh();
                        magnet_units.Add(tActor);
                        tActor.is_in_magnet = true;
                        _picked_up_multiplier = 2f;
                    });
                }
            }

            _has_units = _magnet_units.Count > 0;
        }
    }
}