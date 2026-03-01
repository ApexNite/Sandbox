using Sandbox.UI;

namespace Sandbox.Features {
    internal class ForceCityKingdom {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("force_city_kingdom", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "force_city_kingdom",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = ForceKingdom,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "force_city_kingdom";
            power.drop_id = "force_city_kingdom";
            power.cached_drop_asset = dropAsset;

            ForceCityKingdomSelector.CreateWindow("force_city_kingdom", "force_city_kingdom");
        }

        private static void ForceKingdom(WorldTile worldTile, string dropId) {
            Kingdom kingdom = ForceCityKingdomSelector.LastSelectedKingdom;

            if (kingdom == null) {
                return;
            }

            if (worldTile.hasCity()) {
                worldTile.zone_city.joinAnotherKingdom(kingdom);
            }
        }
    }
}