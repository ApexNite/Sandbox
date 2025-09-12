using Sandbox.UI;

namespace Sandbox.Features {
    internal class BuildingConstructor {
        public static void Init() {
            UpdateAssets();

            BuildingConstructorSelector.CreateWindow("building_constructor", "building_constructor");
        }

        public static void UpdateAssets() {
            foreach (BuildingAsset buildingAsset in AssetManager.buildings.list) {
                string id = $"place_{buildingAsset.id}".Underscore();

                if (AssetManager.powers.dict.ContainsKey(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                }

                GodPower power = AssetManager.powers.clone(id, "$template_drop_building$");
                DropAsset dropAsset = AssetManager.drops.clone(id, "$spawn_building$");

                power.name = id;
                power.drop_id = id;
                power.unselect_when_window = false;
                power.cached_drop_asset = dropAsset;
                dropAsset.building_asset = buildingAsset.id;
                dropAsset.action_landed = SpawnBuilding;
            }
        }

        private static void SpawnBuilding(WorldTile worldTile = null, string dropId = null) {
            string buildingAssetId = AssetManager.drops.get(dropId).building_asset;
            bool doBuildingChecks = Main.Instance.GetConfig()["building_constructor"]["do_building_checks"].BoolVal;
            BuildingAsset buildingAsset = AssetManager.buildings.get(buildingAssetId);
            Building building = World.world.buildings.addBuilding(buildingAsset, worldTile, doBuildingChecks);

            if (building == null) {
                EffectsLibrary.spawnAtTile("fx_bad_place", worldTile, 0.25f);

                return;
            }

            buildingAsset.checkLimits(building);
        }
    }
}