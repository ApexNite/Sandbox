namespace Sandbox.Features {
    internal class MakeUnitLeader {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("make_unit_leader", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "make_unit_leader",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = MakeLeader,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "make_unit_leader";
            power.drop_id = "make_unit_leader";
            power.cached_drop_asset = dropAsset;
        }

        private static void MakeLeader(WorldTile worldTile, string dropId) {
            Actor bestOption = null;

            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (!actor.hasCity()) {
                    continue;
                }

                if (actor.isFavorite()) {
                    bestOption = actor;

                    break;
                }

                if (bestOption == null) {
                    bestOption = actor;
                }
            }

            if (bestOption == null) {
                return;
            }

            bestOption.city.removeLeader();
            bestOption.city.setLeader(bestOption, true);
        }
    }
}