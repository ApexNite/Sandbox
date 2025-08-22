using System.Linq;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using Sandbox.Patches;
using UnityEngine;

namespace Sandbox.UI {
    internal class DisableReligionTraitsWindow : AutoLayoutWindow<DisableReligionTraitsWindow> {
        protected override void Init() {
            AutoGridLayoutGroup grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));

            foreach (ReligionTrait religionTrait in AssetManager.religion_traits.list) {
                string id = $"{religionTrait.id}_religion_trait_toggle";
                bool firstRun = PlayerConfig.instance.data.list.All(data => data.name != id);
                Sprite sprite = SpriteTextureLoader.getSprite(religionTrait.path_icon);

                AssetManager.powers.add(new GodPower { id = id, name = id, toggle_name = id });
                grid.AddChild(PowerButtonCreator.CreateToggleButton(id, sprite).gameObject);

                if (firstRun) {
                    PlayerConfig.setOptionBool(id, true);
                }
            }

            MetaObjectWithTraits_Patch.Patch();
            MetaObjectWithTraits_Patch.DisableReligionTraitsEnabled = true;
        }
    }
}