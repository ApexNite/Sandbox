using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using Sandbox.Features;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class ForceUnitJobSelector : AutoLayoutWindow<ForceUnitJobSelector> {
        private AutoGridLayoutGroup _grid;
        private List<string> _loadedJobs;

        public override void OnNormalEnable() {
            ForceUnitProfession.UpdateAssets();
            UpdateButtons();
        }

        protected override void Init() {
            _grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));
            _loadedJobs = new List<string>();

            UpdateButtons();
        }

        private void UpdateButtons() {
            foreach (CitizenJobAsset jobAsset in AssetManager.citizen_job_library.list) {
                string id = $"j_make_unit_{jobAsset.id}".Underscore();

                if (_loadedJobs.Contains(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                    LocalizedTextManager.add($"{id}_description", $"{id}_description");
                }

                Sprite sprite = SpriteTextureLoader.getSprite(jobAsset.path_icon);

                if (id == "j_make_unit_manure_cleaner") {
                    sprite = SpriteTextureLoader.getSprite("ui/Icons/citizen_jobs/iconCitizenJobCleaner");
                }

                if (sprite == null) {
                    sprite = SpriteTextureLoader.getSprite("ui/icons/iconQuestionMark");
                }

                PowerButton powerButton = PowerButtonCreator.CreateGodPowerButton(id, sprite);
                powerButton.gameObject.GetComponent<Button>()
                    .onClick.AddListener(() => ScrollWindow.getCurrentWindow().clickHide());

                _grid.AddChild(powerButton.gameObject);
                _loadedJobs.Add(id);
            }
        }
    }
}