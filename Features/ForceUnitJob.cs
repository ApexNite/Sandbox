using Sandbox.UI;

namespace Sandbox.Features {
    internal class ForceUnitJob {
        public static void Init() {
            UpdateAssets();

            ForceUnitJobSelector.CreateWindow("force_unit_job", "force_unit_job");
        }

        public static void UpdateAssets() {
            foreach (CitizenJobAsset jobAsset in AssetManager.citizen_job_library.list) {
                string id = $"j_make_unit_{jobAsset.id}".Underscore();

                if (AssetManager.powers.dict.ContainsKey(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                }

                GodPower power = AssetManager.powers.clone(id, "$template_drops$");
                DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                    id = id,
                    path_texture = "drops/drop_gamma_rain",
                    random_frame = true,
                    default_scale = 0.1f,
                    action_landed = ForceJob,
                    material = "mat_world_object_lit",
                    sound_drop = "event:/SFX/DROPS/DropRainGamma",
                    type = DropType.DropGeneric
                });

                power.name = id;
                power.drop_id = id;
                power.cached_drop_asset = dropAsset;
                power.unselect_when_window = false;
            }
        }

        private static void ForceJob(WorldTile worldTile, string dropId) {
            CitizenJobAsset jobAsset = AssetManager.citizen_job_library.get(dropId.Remove(0, 12));

            if (jobAsset == null) {
                return;
            }

            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (!actor.hasCity()) {
                    continue;
                }

                actor.setCitizenJob(jobAsset);
            }
        }
    }
}