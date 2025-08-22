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
            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (ForceUnitCitySelector.LastSelectedCity != null) {
                    actor.joinCity(ForceUnitCitySelector.LastSelectedCity);
                }
            }
        }
    }
}