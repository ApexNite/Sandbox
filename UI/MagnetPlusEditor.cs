using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;

namespace Sandbox.UI {
    internal class MagnetPlusEditor : AutoLayoutWindow<MagnetPlusEditor> {
        protected override void Init() {
            AutoGridLayoutGroup grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));

            foreach (ActorAsset actorAsset in AssetManager.actor_library.list) {
                if (!actorAsset.can_be_moved_by_powers || actorAsset.id == "fire_elemental") {
                    continue;
                }

                string id = $"{actorAsset.id}_magnet_toggle";
                Sprite sprite;

                if (actorAsset.id == "zombie_dragon") {
                    sprite = PrefabLibrary.instance.zombieDragonAsset.getAsset(DragonState.Idle).frames[0];
                } else if (actorAsset.id == "god_finger") {
                    sprite = SpriteTextureLoader.getSprite("ui/icons/iconGodFinger");
                } else if (actorAsset.is_boat) {
                    sprite = GetBoatSprite(actorAsset);
                } else if (HasNoUniqueIcon(actorAsset)) {
                    sprite = GetAssetIdleSprite(actorAsset);
                } else {
                    sprite = actorAsset.getSpriteIcon();
                }

                AssetManager.powers.add(new GodPower { id = id, name = id, toggle_name = id });
                grid.AddChild(PowerButtonCreator.CreateToggleButton(id, sprite).gameObject);
            }
        }

        private static Sprite GetAssetIdleSprite(ActorAsset actorAsset) {
            string path = actorAsset.texture_asset.texture_path_base;
            AnimationContainerUnit animation = new AnimationContainerUnit(path);

            return animation.sprites[actorAsset.animation_idle[0]];
        }

        private static Sprite GetBoatSprite(ActorAsset actorAsset) {
            return SpriteTextureLoader.getSpriteList($"actors/boats/{actorAsset.boat_texture_id}")[0];
        }

        private static bool HasNoUniqueIcon(ActorAsset actorAsset) {
            return actorAsset.unit_zombie && actorAsset.id != "zombie"
                   || actorAsset.kingdom_id_wild == "mush"
                   || actorAsset.kingdom_id_wild == "tumor"
                   || actorAsset.base_asset_id == "fire_elemental";
        }
    }
}