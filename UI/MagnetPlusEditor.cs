using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;

namespace Sandbox.UI {
    internal class MagnetPlusEditor : AutoLayoutWindow<MagnetPlusEditor> {
        private AutoGridLayoutGroup _grid;
        private List<string> _loadedActors;

        public override void OnNormalEnable() {
            UpdateButtons();
        }

        protected override void Init() {
            _grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));
            _loadedActors = new List<string>();

            UpdateButtons();
        }

        private static Sprite GetAssetIdleSprite(ActorAsset actorAsset) {
            string path = actorAsset.texture_asset.texture_path_base;
            AnimationContainerUnit animation = new AnimationContainerUnit(path);

            return actorAsset.animation_idle.Length == 0 ? null : animation.sprites[actorAsset.animation_idle[0]];
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

        private void UpdateButtons() {
            foreach (ActorAsset actorAsset in AssetManager.actor_library.list) {
                if (!actorAsset.can_be_moved_by_powers || actorAsset.id == "fire_elemental") {
                    continue;
                }

                string id = $"{actorAsset.id}_magnet_toggle";

                if (_loadedActors.Contains(id)) {
                    continue;
                }

                if (!LocalizedTextManager.instance.contains(id)) {
                    LocalizedTextManager.add(id, id);
                    LocalizedTextManager.add($"{id}_description", $"{id}_description");
                }

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

                if (sprite == null) {
                    sprite = SpriteTextureLoader.getSprite("ui/icons/iconQuestionMark");
                }

                AssetManager.powers.add(new GodPower { id = id, name = id, toggle_name = id });
                _grid.AddChild(PowerButtonCreator.CreateToggleButton(id, sprite).gameObject);
                _loadedActors.Add(id);
            }
        }
    }
}