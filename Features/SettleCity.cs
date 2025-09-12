using UnityEngine;

namespace Sandbox.Features {
    internal class SettleCity {
        private static City _lastCityFounded;
        private static Kingdom _selectedKingdom;

        public static void Init() {
            GodPower power = new GodPower {
                id = "settle_city",
                name = "settle_city",
                force_map_mode = MetaType.Kingdom,
                can_drag_map = true,
                select_button_action = SelectSettleCity,
                click_special_action = ClickSettleCity
            };
            QuantumSpriteAsset quantumSprite = new QuantumSpriteAsset {
                id = "settle_city_line",
                id_prefab = "p_mapArrow_line",
                base_scale = 0.5f,
                draw_call = DrawSettleLine,
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

        private static bool ClickSettleCity(WorldTile worldTile, string powerId) {
            if (_selectedKingdom == null) {
                if (worldTile.zone.city == null) {
                    return false;
                }

                if (worldTile.zone.city.isRekt()) {
                    return false;
                }

                if (worldTile.zone.city.kingdom.isRekt()) {
                    return false;
                }

                if (worldTile.zone.city.kingdom.isNeutral()) {
                    return false;
                }

                _selectedKingdom = worldTile.zone.city.kingdom;

                ShowSettleTip("settle_kingdom_selected");

                return false;
            }

            if (worldTile.zone.city != null && _selectedKingdom == worldTile.zone.city.kingdom) {
                ShowSettleTip("settle_cancelled");

                _selectedKingdom = null;

                return false;
            }

            string kingdomSpecies = _selectedKingdom.getSpecies();
            Subspecies kingdomSubspecies = World.world.subspecies.get(_selectedKingdom.getMainSubspecies().id);
            Actor actor = World.world.units.createNewUnit(kingdomSpecies, worldTile, true, 3f, kingdomSubspecies);

            actor.joinKingdom(_selectedKingdom);
            _lastCityFounded = World.world.cities.buildNewCity(actor, worldTile.zone);
            actor.joinCity(_lastCityFounded);
            ShowSettleTip("settle_settled");

            _selectedKingdom = null;

            return true;
        }

        private static void DrawSettleLine(QuantumSpriteAsset pAsset) {
            if (!InputHelpers.mouseSupported) {
                return;
            }

            if (World.world.isBusyWithUI()) {
                return;
            }

            if (!World.world.isSelectedPower("settle_city")) {
                return;
            }

            if (_selectedKingdom == null) {
                return;
            }

            Vector3 capitalPos = _selectedKingdom.capital.getTile().posV;
            Vector2 mousePos = World.world.getMousePos();
            Color color = _selectedKingdom.getColor().getColorMainSecond();

            QuantumSpriteLibrary.drawArrowQuantumSprite(pAsset, capitalPos, mousePos, ref color);
        }

        private static bool SelectSettleCity(string powerId) {
            ShowSettleTip("settle_selection");
            _selectedKingdom = null;

            return false;
        }

        private static void ShowSettleTip(string text) {
            string localizedText = LocalizedTextManager.getText(text);

            if (_selectedKingdom != null) {
                localizedText = localizedText.Replace("$kingdom_name$", _selectedKingdom.name);
            }

            if (_lastCityFounded != null) {
                localizedText = localizedText.Replace("$city_name$", _lastCityFounded.name);
                _lastCityFounded = null;
            }

            WorldTip.showNow(localizedText, false, "top");
        }
    }
}