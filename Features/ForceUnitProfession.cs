using Sandbox.UI;

namespace Sandbox.Features {
    internal class ForceUnitProfession {
        public static void Init() {
            UpdateAssets();

            ForceUnitProfessionSelector.CreateWindow("force_unit_profession", "force_unit_profession");
        }

        public static void UpdateAssets() {
            foreach (ProfessionAsset professionAsset in AssetManager.professions.list) {
                string id = $"make_unit_{professionAsset.id}".Underscore();

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
                    action_landed = ForceProfession,
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

        private static void ForceProfession(WorldTile worldTile, string dropId) {
            switch (dropId) {
                case "make_unit_king":
                    MakeKing(worldTile, dropId);

                    return;
                case "make_unit_leader":
                    MakeLeader(worldTile, dropId);

                    return;
                default:
                    ProfessionAsset profession = AssetManager.professions.get(dropId.Remove(0, 10));

                    if (profession == null) {
                        return;
                    }

                    foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                        if (!actor.hasCity()) {
                            continue;
                        }

                        actor.setProfession(profession.profession_id);
                    }

                    break;
            }
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

            if (bestOption == null) {
                return;
            }

            bestOption.kingdom.removeKing();
            bestOption.kingdom.setKing(bestOption);
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