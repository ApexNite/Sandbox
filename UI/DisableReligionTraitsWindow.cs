using System.Collections.Generic;
using System.Linq;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using Sandbox.Patches;
using UnityEngine;

namespace Sandbox.UI {
    internal class DisableReligionTraitsWindow : AutoLayoutWindow<DisableReligionTraitsWindow> {
        private static DisableReligionTraitsWindow _instance;
        private AutoGridLayoutGroup _grid;
        private List<string> _loadedTraits;

        public static bool ReligionTraitEnabled(string traitId) {
            string id = $"{traitId}_religion_trait_toggle";

            if (!PlayerConfig.dict.ContainsKey(id)) {
                _instance.UpdateButtons();
            }

            return PlayerConfig.dict.ContainsKey(id) && PlayerConfig.optionBoolEnabled(id);
        }

        public override void OnNormalEnable() {
            UpdateButtons();
        }

        protected override void Init() {
            _grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));
            _loadedTraits = new List<string>();

            UpdateButtons();

            MetaObjectWithTraits_Patch.Patch();
            MetaObjectWithTraits_Patch.DisableReligionTraitsEnabled = true;

            _instance = this;
        }

        private void UpdateButtons() {
            foreach (ReligionTrait religionTrait in AssetManager.religion_traits.list) {
                string id = $"{religionTrait.id}_religion_trait_toggle";

                if (_loadedTraits.Contains(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                    LocalizedTextManager.add($"{id}_description", $"{id}_description");
                }

                bool firstRun = PlayerConfig.instance.data.list.All(data => data.name != id);
                Sprite sprite = SpriteTextureLoader.getSprite(religionTrait.path_icon);

                if (sprite == null) {
                    sprite = SpriteTextureLoader.getSprite("ui/icons/iconQuestionMark");
                }

                AssetManager.powers.add(new GodPower { id = id, name = id, toggle_name = id });
                _grid.AddChild(PowerButtonCreator.CreateToggleButton(id, sprite).gameObject);
                _loadedTraits.Add(id);

                if (firstRun) {
                    PlayerConfig.setOptionBool(id, true);
                }
            }
        }
    }
}