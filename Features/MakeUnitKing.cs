namespace Sandbox.Features {
    internal class MakeUnitKing {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("make_unit_king", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "make_unit_king",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = MakeKing,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "make_unit_king";
            power.drop_id = "make_unit_king";
            power.cached_drop_asset = dropAsset;
        }

        private static void MakeKing(WorldTile worldTile, string dropId) {
            Actor bestOption = null;

            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (!actor.hasKingdom() || actor.isNomad()) {
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

            if (bestOption != null) {
                bestOption.kingdom.setKing(bestOption);
            }
        }
    }
}