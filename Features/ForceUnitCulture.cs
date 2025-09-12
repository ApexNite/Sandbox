using NeoModLoader.api;
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
            Culture culture = ForceUnitCultureSelector.LastSelectedCulture;
            ModConfig config = Main.Instance.GetConfig();

            if (culture == null) {
                return;
            }

            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (config["force_unit_culture"]["culture_check_culture"].BoolVal && actor.hasCulture()) {
                    continue;
                }

                if (config["force_unit_culture"]["sapient_check_culture"].BoolVal && !actor.isSapient()) {
                    continue;
                }

                if (config["force_unit_culture"]["advanced_memory_check_culture"].BoolVal
                    && actor.hasSubspecies()
                    && !actor.subspecies.has_advanced_memory) {
                    continue;
                }

                if (config["force_unit_culture"]["true_roots_check_culture"].BoolVal
                    && actor.hasCulture()
                    && actor.culture.hasTrueRoots()) {
                    continue;
                }

                actor.setCulture(culture);
            }
        }
    }
}