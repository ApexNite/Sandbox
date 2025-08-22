using System.Linq;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using Sandbox.Patches;
using UnityEngine;

namespace Sandbox.UI {
    internal class DisableCultureTraitsWindow : AutoLayoutWindow<DisableCultureTraitsWindow> {
        protected override void Init() {
            AutoGridLayoutGroup grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));

            foreach (CultureTrait cultureTrait in AssetManager.culture_traits.list) {
                string id = $"{cultureTrait.id}_culture_trait_toggle";
                bool firstRun = PlayerConfig.instance.data.list.All(data => data.name != id);
                Sprite sprite = SpriteTextureLoader.getSprite(cultureTrait.path_icon);

                AssetManager.powers.add(new GodPower { id = id, name = id, toggle_name = id });
                grid.AddChild(PowerButtonCreator.CreateToggleButton(id, sprite).gameObject);

                if (firstRun) {
                    PlayerConfig.setOptionBool(id, true);
                }
            }

            MetaObjectWithTraits_Patch.Patch();
            MetaObjectWithTraits_Patch.DisableCultureTraitsEnabled = true;
        }
    }
}