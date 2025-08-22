using Sandbox.UI;

namespace Sandbox.Features {
    internal class BuildingConstructor {
        public static void Init() {
            foreach (BuildingAsset buildingAsset in AssetManager.buildings.list) {
                string id = $"place_{buildingAsset.id}";

                GodPower power = AssetManager.powers.clone(id, "$template_drop_building$");
                DropAsset dropAsset = AssetManager.drops.clone(id, "$spawn_building$");

                power.name = id;
                power.drop_id = id;
                power.unselect_when_window = false;
                power.cached_drop_asset = dropAsset;
                dropAsset.building_asset = buildingAsset.id;
                dropAsset.action_landed = DropsLibrary.action_spawn_building;
            }

            BuildingConstructorSelector.CreateWindow("building_constructor", "building_constructor");
        }
    }
}