using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using Sandbox.Features;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class BuildingConstructorSelector : AutoLayoutWindow<BuildingConstructorSelector> {
        private AutoGridLayoutGroup _grid;
        private List<string> _loadedBuildings;

        public override void OnNormalEnable() {
            BuildingConstructor.UpdateAssets();
            UpdateButtons();
        }

        protected override void Init() {
            _grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));
            _loadedBuildings = new List<string>();

            UpdateButtons();
        }

        private void UpdateButtons() {
            foreach (BuildingAsset buildingAsset in AssetManager.buildings.list) {
                string id = $"place_{buildingAsset.id}";

                if (_loadedBuildings.Contains(id)) {
                    continue;
                }

                buildingAsset.checkSpritesAreLoaded();

                Sprite sprite = SpriteTextureLoader.getSprite("ui/icons/iconQuestionMark");
                List<BuildingAnimationData> animations = buildingAsset.building_sprites.animation_data;

                if (animations != null && animations.Count > 0) {
                    BuildingAnimationData animationData = animations[0];

                    if (animationData != null && animationData.main.Length > 0) {
                        sprite = animationData.main.GetRandom();
                    }
                }

                PowerButton powerButton = PowerButtonCreator.CreateGodPowerButton(id, sprite);
                powerButton.gameObject.GetComponent<Button>()
                    .onClick.AddListener(() => ScrollWindow.getCurrentWindow().clickHide());

                _grid.AddChild(powerButton.gameObject);
                _loadedBuildings.Add(id);
            }
        }
    }
}