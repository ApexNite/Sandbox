using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class BuildingConstructorSelector : AutoLayoutWindow<BuildingConstructorSelector> {
        protected override void Init() {
            AutoGridLayoutGroup grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));

            foreach (BuildingAsset buildingAsset in AssetManager.buildings.list) {
                string id = $"place_{buildingAsset.id}";

                buildingAsset.checkSpritesAreLoaded();

                Sprite sprite = buildingAsset.building_sprites.animation_data.GetRandom().main.GetRandom();
                PowerButton powerButton = PowerButtonCreator.CreateGodPowerButton(id, sprite);
                powerButton.gameObject.GetComponent<Button>()
                    .onClick.AddListener(() => ScrollWindow.getCurrentWindow().clickHide());

                grid.AddChild(powerButton.gameObject);
            }
        }
    }
}