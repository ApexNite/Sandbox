using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sandbox.Features {
    internal class MergeCities {
        private static City _lastMergedCity;
        private static City _selectedCity1;
        private static City _selectedCity2;

        public static void Init() {
            GodPower power = new GodPower {
                id = "merge_cities",
                name = "merge_cities",
                force_map_mode = MetaType.City,
                can_drag_map = true,
                select_button_action = SelectMergeCity,
                click_special_action = ClickMergeCity
            };
            QuantumSpriteAsset quantumSprite = new QuantumSpriteAsset {
                id = "merge_city_line",
                id_prefab = "p_mapArrow_line",
                base_scale = 0.5f,
                draw_call = DrawMergeLine,
                render_map = true,
                render_gameplay = true,
                color = new Color(0.4f, 0.4f, 1f, 0.9f)
            };

            AssetManager.quantum_sprites.add(quantumSprite);
            AssetManager.powers.add(power);

            QuantumSpriteGroupSystem groupSystem = new GameObject().AddComponent<QuantumSpriteGroupSystem>();
            groupSystem.create(quantumSprite);
            quantumSprite.group_system = groupSystem;
            quantumSprite.group_system.turn_off_renderer = quantumSprite.turn_off_renderer;
        }

        private static bool ClickMergeCity(WorldTile worldTile, string powerId) {
            if (_selectedCity1 == null) {
                if (worldTile.zone_city == null) {
                    return false;
                }

                if (worldTile.zone_city.isRekt()) {
                    return false;
                }

                _selectedCity1 = worldTile.zone_city;

                ShowMergeTip("merge_city_selected");

                return false;
            }

            if (worldTile.zone_city == null || _selectedCity1 == worldTile.zone_city) {
                ShowMergeTip("merge_canceled");

                _selectedCity1 = null;

                return false;
            }

            _selectedCity2 = worldTile.zone_city;

            List<TileZone> zones = new List<TileZone>(_selectedCity1.zones);
            List<Actor> actors = new List<Actor>(_selectedCity1.units);
            List<Actor> boats = new List<Actor>(_selectedCity1._boats);
            List<List<Item>> equipmentLists = new List<List<Item>>(_selectedCity1.data.equipment.getAllEquipmentLists()
                .Select(l => l.Select(i => World.world.items.get(i)).ToList()));

            _selectedCity1.destroyCity();

            foreach (TileZone tileZone in zones) {
                _selectedCity2.addZone(tileZone);
            }

            foreach (Actor actor in actors) {
                actor.setCity(_selectedCity2);
            }

            foreach (Actor boat in boats) {
                boat.setCity(_selectedCity2);
            }

            foreach (List<Item> equipmentList in equipmentLists) {
                _selectedCity2.tryToPutItems(equipmentList);
            }

            ShowMergeTip("merge_merged");

            _selectedCity1 = null;
            _selectedCity2 = null;

            return true;
        }

        private static void DrawMergeLine(QuantumSpriteAsset pAsset) {
            if (!InputHelpers.mouseSupported) {
                return;
            }

            if (World.world.isBusyWithUI()) {
                return;
            }

            if (!World.world.isSelectedPower("merge_cities")) {
                return;
            }

            if (_selectedCity1 == null) {
                return;
            }

            Vector3 capitalPos = _selectedCity1.getTile().posV;
            Vector2 mousePos = World.world.getMousePos();
            Color color = _selectedCity1.getColor().getColorMainSecond();

            QuantumSpriteLibrary.drawArrowQuantumSprite(pAsset, capitalPos, mousePos, ref color);
        }

        private static bool SelectMergeCity(string powerId) {
            ShowMergeTip("merge_selection");
            _selectedCity1 = null;

            return false;
        }

        private static void ShowMergeTip(string text) {
            string localizedText = LocalizedTextManager.getText(text);

            if (_selectedCity1 != null) {
                localizedText = localizedText.Replace("$city1_name$", _selectedCity1.name);
            }

            if (_selectedCity2 != null) {
                localizedText = localizedText.Replace("$city2_name$", _selectedCity2.name);
            }

            if (_lastMergedCity != null) {
                localizedText = localizedText.Replace("$merged_city_name$", _lastMergedCity.name);
                _lastMergedCity = null;
            }

            WorldTip.showNow(localizedText, false, "top");
        }
    }
}