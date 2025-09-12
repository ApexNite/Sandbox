using System.Collections.Generic;
using System.Linq;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using Sandbox.Patches;
using UnityEngine;

namespace Sandbox.UI {
    internal class DisableLanguageTraitsWindow : AutoLayoutWindow<DisableLanguageTraitsWindow> {
        private static DisableLanguageTraitsWindow _instance;
        private AutoGridLayoutGroup _grid;
        private List<string> _loadedTraits;

        public static bool LanguageTraitEnabled(string traitId) {
            string id = $"{traitId}_language_trait_toggle";

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
            MetaObjectWithTraits_Patch.DisableLanguageTraitsEnabled = true;

            _instance = this;
        }

        private void UpdateButtons() {
            foreach (LanguageTrait languageTrait in AssetManager.language_traits.list) {
                string id = $"{languageTrait.id}_language_trait_toggle";

                if (_loadedTraits.Contains(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                    LocalizedTextManager.add($"{id}_description", $"{id}_description");
                }

                bool firstRun = PlayerConfig.instance.data.list.All(data => data.name != id);
                Sprite sprite = SpriteTextureLoader.getSprite(languageTrait.path_icon);

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