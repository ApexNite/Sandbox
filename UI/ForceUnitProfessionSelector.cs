using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using Sandbox.Features;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class ForceUnitProfessionSelector : AutoLayoutWindow<ForceUnitProfessionSelector> {
        private readonly Dictionary<string, string> _sprites = new Dictionary<string, string> {
            { "make_unit_nothing", "ui/icons/profession_nothing_icon" },
            { "make_unit_king", "ui/icons/force_unit_king_icon" },
            { "make_unit_leader", "ui/icons/force_unit_leader_icon" },
            { "make_unit_warrior", "ui/icons/profession_warrior_icon" },
            { "make_unit_unit", "ui/icons/iconPopulation" }
        };

        private AutoGridLayoutGroup _grid;
        private List<string> _loadedProfessions;

        public override void OnNormalEnable() {
            ForceUnitProfession.UpdateAssets();
            UpdateButtons();
        }

        protected override void Init() {
            _grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));
            _loadedProfessions = new List<string>();

            UpdateButtons();
        }

        private void UpdateButtons() {
            foreach (ProfessionAsset professionAsset in AssetManager.professions.list) {
                string id = $"make_unit_{professionAsset.id}".Underscore();

                if (_loadedProfessions.Contains(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                    LocalizedTextManager.add($"{id}_description", $"{id}_description");
                }

                Sprite sprite = SpriteTextureLoader.getSprite("ui/icons/iconQuestionMark");

                if (_sprites.TryGetValue(id, out string spriteId)) {
                    sprite = SpriteTextureLoader.getSprite(spriteId);
                }

                PowerButton powerButton = PowerButtonCreator.CreateGodPowerButton(id, sprite);
                powerButton.gameObject.GetComponent<Button>()
                    .onClick.AddListener(() => ScrollWindow.getCurrentWindow().clickHide());

                _grid.AddChild(powerButton.gameObject);
                _loadedProfessions.Add(id);
            }
        }
    }
}