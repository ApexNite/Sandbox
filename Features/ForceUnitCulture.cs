using Sandbox.UI;

namespace Sandbox.Features {
    internal class ForceUnitCulture {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("force_unit_culture", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "force_unit_culture",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = ForceCulture,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "force_unit_culture";
            power.drop_id = "force_unit_culture";
            power.cached_drop_asset = dropAsset;

            ForceUnitCultureSelector.CreateWindow("force_unit_culture", "force_unit_culture");
        }

        private static void ForceCulture(WorldTile worldTile, string dropId) {
            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (ForceUnitCultureSelector.LastSelectedCulture != null) {
                    actor.setCulture(ForceUnitCultureSelector.LastSelectedCulture);
                }
            }
        }
    }
}