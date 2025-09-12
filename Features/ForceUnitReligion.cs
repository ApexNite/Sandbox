using NeoModLoader.api;
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
            Religion religion = ForceUnitReligionSelector.LastSelectedReligion;
            ModConfig config = Main.Instance.GetConfig();

            if (religion == null) {
                return;
            }

            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (config["force_unit_religion"]["religion_check_religion"].BoolVal && actor.hasReligion()) {
                    continue;
                }

                if (config["force_unit_religion"]["sapient_check_religion"].BoolVal && !actor.isSapient()) {
                    continue;
                }

                if (config["force_unit_religion"]["advanced_memory_check_religion"].BoolVal
                    && actor.hasSubspecies()
                    && !actor.subspecies.has_advanced_memory) {
                    continue;
                }

                if (config["force_unit_religion"]["true_roots_check_religion"].BoolVal
                    && actor.hasCulture()
                    && actor.culture.hasTrueRoots()) {
                    continue;
                }

                actor.setReligion(ForceUnitReligionSelector.LastSelectedReligion);
            }
        }
    }
}