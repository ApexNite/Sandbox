using Sandbox.UI;

namespace Sandbox.Features {
    internal class ForceUnitReligion {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("force_unit_religion", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "force_unit_religion",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = ForceReligion,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "force_unit_religion";
            power.drop_id = "force_unit_religion";
            power.cached_drop_asset = dropAsset;

            ForceUnitReligionSelector.CreateWindow("force_unit_religion", "force_unit_religion");
        }

        private static void ForceReligion(WorldTile worldTile, string dropId) {
            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (ForceUnitReligionSelector.LastSelectedReligion != null) {
                    actor.setReligion(ForceUnitReligionSelector.LastSelectedReligion);
                }
            }
        }
    }
}