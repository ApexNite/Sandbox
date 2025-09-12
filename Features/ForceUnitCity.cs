using NeoModLoader.api;
using Sandbox.UI;

namespace Sandbox.Features {
    internal class ForceUnitCity {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("force_unit_city", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "force_unit_city",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = ForceCity,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "force_unit_city";
            power.drop_id = "force_unit_city";
            power.cached_drop_asset = dropAsset;

            ForceUnitCitySelector.CreateWindow("force_unit_city", "force_unit_city");
        }

        private static void ForceCity(WorldTile worldTile, string dropId) {
            City city = ForceUnitCitySelector.LastSelectedCity;
            ModConfig config = Main.Instance.GetConfig();

            if (city == null) {
                return;
            }

            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (config["force_unit_city"]["city_check_city"].BoolVal && actor.hasCity()) {
                    continue;
                }

                if (config["force_unit_city"]["sapient_check_city"].BoolVal && !actor.isSapient()) {
                    continue;
                }

                if (config["force_unit_city"]["species_check_city"].BoolVal && !city.isSameSpeciesAsActor(actor)) {
                    continue;
                }

                if (config["force_unit_city"]["subspecies_check_city"].BoolVal
                    && !actor.isSameSubspecies(city.getMainSubspecies())) {
                    continue;
                }

                if (config["force_unit_city"]["king_check_city"].BoolVal && actor.isKing()) {
                    continue;
                }

                if (config["force_unit_city"]["leader_check_city"].BoolVal && actor.isCityLeader()) {
                    continue;
                }

                if (config["force_unit_city"]["population_check_city"].BoolVal
                    && city.getPopulationPeople() >= city.getPopulationMaximum()) {
                    continue;
                }

                actor.joinCity(city);
            }
        }
    }
}