using NeoModLoader.api;
using Sandbox.UI;

namespace Sandbox.Features {
    internal class ForceUnitLanguage {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("force_unit_language", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "force_unit_language",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = ForceLanguage,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "force_unit_language";
            power.drop_id = "force_unit_language";
            power.cached_drop_asset = dropAsset;

            ForceUnitLanguageSelector.CreateWindow("force_unit_language", "force_unit_language");
        }

        private static void ForceLanguage(WorldTile worldTile, string dropId) {
            Language language = ForceUnitLanguageSelector.LastSelectedLanguage;
            ModConfig config = Main.Instance.GetConfig();

            if (language == null) {
                return;
            }

            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (config["force_unit_language"]["language_check_language"].BoolVal && actor.hasLanguage()) {
                    continue;
                }

                if (config["force_unit_language"]["sapient_check_language"].BoolVal && !actor.isSapient()) {
                    continue;
                }

                if (config["force_unit_language"]["advanced_communication_check_language"].BoolVal
                    && actor.hasSubspecies()
                    && !actor.subspecies.has_advanced_communication) {
                    continue;
                }

                if (config["force_unit_language"]["true_roots_check_language"].BoolVal
                    && actor.hasCulture()
                    && actor.culture.hasTrueRoots()) {
                    continue;
                }

                actor.joinLanguage(language);
            }
        }
    }
}