// By AvoidingBoredom with minor modifications 

namespace Sandbox.Features {
    internal class DirectSettleCity {
        private static City _lastCityFounded;
        private static Kingdom _selectedKingdom;

        public static void Init() {
            GodPower power = new GodPower {
                id = "direct_settle_city",
                name = "direct_settle_city",
                click_action = ClickSettleCity,
                allow_unit_selection = false
            };

            AssetManager.powers.add(power);
        }

        private static void BuildCityAndStartCivilizationCustom(Actor actor) {
            if (!CanStartNewCityCivilizationHereCustom(actor)) {
                return;
            }

            _selectedKingdom = World.world.kingdoms.makeNewCivKingdom(actor);

            if (_selectedKingdom == null) {
                return;
            }

            _lastCityFounded = World.world.cities.buildFirstCivilizationCity(actor);
            actor.createDefaultCultureAndLanguageAndClan(_lastCityFounded.name);
            _selectedKingdom.setUnitMetas(actor);
            _lastCityFounded.setUnitMetas(actor);
        }

        private static bool CanBuildNewCityCustom(Actor pActor) {
            int counter = 0;

            if (!pActor.current_zone.hasCity()) {
                counter++;
            }

            if (!pActor.hasCity()) {
                counter++;
            }

            if (IsGoodForNewCityCustom(pActor, pActor.current_zone)) {
                counter++;
            }

            return counter == 3;
        }

        private static bool CanStartNewCityCivilizationHereCustom(Actor pActor) {
            if (pActor.kingdom.asset.is_forced_by_trait) {
                return false;
            }

            if (!CanBuildNewCityCustom(pActor)) {
                return false;
            }

            KingdomAsset tPossibleKingdomAsset = AssetManager.kingdoms.get(pActor.asset.kingdom_id_civilization);

            return tPossibleKingdomAsset != null && tPossibleKingdomAsset.civ;
        }

        private static bool ClickSettleCity(WorldTile worldTile, string powerId) {
            Actor actor = ActionLibrary.getActorFromTile(worldTile);

            if (actor == null || actor.asset.id == null || !actor.subspecies.isSapient()) {
                ShowSettleTip("settle_invalid_usage");

                return false;
            }

            if (actor.kingdom.isCiv()) {
                if (worldTile.zone.city != null && actor.kingdom == worldTile.zone.city.kingdom) {
                    ShowSettleTip("settle_own_city");

                    return false;
                }

                if (worldTile.zone.city != null && actor.kingdom != worldTile.zone.city.kingdom) {
                    worldTile.zone.city.joinAnotherKingdom(actor.kingdom, true);

                    if (actor.city.getPopulationPeople() == 1 || actor.isKing()) {
                        Actor actorCreateToPreserveOgE =
                            World.world.units.createNewUnit(actor.asset.id, worldTile, true, 3f, actor.subspecies);
                        actorCreateToPreserveOgE.joinCity(worldTile.zone.city);
                        ShowSettleTip("settle_change_city");
                    } else {
                        actor.joinCity(worldTile.zone.city);
                        ShowSettleTip("settle_change_city");
                    }

                    return false;
                }

                _selectedKingdom = actor.kingdom;
                _lastCityFounded = World.world.cities.buildNewCity(actor, worldTile.zone);

                if (actor.city.getPopulationPeople() == 1 || actor.isKing()) {
                    Actor actorCreateToPreserveOgN =
                        World.world.units.createNewUnit(actor.asset.id, worldTile, true, 3f, actor.subspecies);
                    actorCreateToPreserveOgN.joinCity(worldTile.zone.city);
                    ShowSettleTip("settle_settled");
                } else {
                    actor.joinCity(worldTile.zone.city);
                    ShowSettleTip("settle_settled");
                }

                return true;
            }

            if (worldTile.zone.city != null) {
                ShowSettleTip("settle_city_already_exists");

                return false;
            }

            BuildCityAndStartCivilizationCustom(actor);

            ShowSettleTip(actor.city == null ? "settle_failed" : "settle_new_kingdom");

            return true;
        }

        private static bool IsGoodForNewCityCustom(TileZone pZone) {
            if (pZone.hasCity()) {
                return false;
            }

            if (pZone._good_for_new_city) {
                return pZone._good_for_new_city;
            }

            pZone.setGoodForNewCity(true);

            return pZone._good_for_new_city;
        }

        private static bool IsGoodForNewCityCustom(Actor pActor, TileZone pZone) {
            if (!pZone._city_place_finder.isDirty()) {
                return IsGoodForNewCityCustom(pZone) && IsGoodForNewCityCustom(pZone);
            }

            if (pZone.hasCity()) {
                return false;
            }

            if (pActor.hasCity() && pActor.city.neighbour_zones.Contains(pZone)) {
                return false;
            }

            return IsGoodForNewCityCustom(pZone) && IsGoodForNewCityCustom(pZone);
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