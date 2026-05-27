namespace Sandbox.Features {
    internal class ForceCityCapital {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("force_capital", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "force_capital",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = ForceCapital,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "force_capital";
            power.drop_id = "force_capital";
            power.cached_drop_asset = dropAsset;
        }

        private static void ForceCapital(WorldTile worldTile, string dropId) {
            if (worldTile == null || !worldTile.hasCity()) {
                return;
            }

            City city = worldTile.zone_city;

            if (city.isRekt()) {
                return;
            }

            Kingdom kingdom = city.kingdom;

            kingdom?.setCapital(city);
        }
    }
}