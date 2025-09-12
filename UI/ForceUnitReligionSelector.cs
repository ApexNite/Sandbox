using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class ForceUnitReligionSelector : AutoLayoutWindow<ForceUnitReligionSelector> {
        public static Religion LastSelectedReligion;
        private PowerButton _forceReligionButton;
        private GameObject _religionElementPrefab;
        private List<GameObject> _religionElements;
        private AutoVertLayoutGroup _vertLayoutGroup;

        public override void OnNormalDisable() {
            foreach (GameObject religionElement in _religionElements) {
                religionElement.SetActive(false);
            }
        }

        public override void OnNormalEnable() {
            int elementIndex = 0;

            foreach (Religion religion in World.world.religions) {
                if (elementIndex >= _religionElements.Count) {
                    GameObject religionElement = Instantiate(_religionElementPrefab);

                    religionElement.GetComponent<Button>()
                        .onClick.AddListener(() => {
                            ScrollWindow.getCurrentWindow().clickHide();
                            _forceReligionButton.clickButton();
                        });

                    _vertLayoutGroup.AddChild(religionElement);
                    _religionElements.Add(religionElement);
                }

                _religionElements[elementIndex].SetActive(true);
                _religionElements[elementIndex].GetComponent<ReligionVisualElement>().SetReligion(religion);
                elementIndex++;
            }
        }

        protected override void Init() {
            _vertLayoutGroup = this.BeginVertGroup();
            _forceReligionButton = PowerButtonCreator.CreateGodPowerButton("force_unit_religion",
                SpriteTextureLoader.getSprite("ui/icons/force_religion_icon"));
            _religionElementPrefab = new GameObject("religionElementPrefab", typeof(Image), typeof(Button),
                typeof(ReligionVisualElement));
            _religionElements = new List<GameObject>();

            _religionElementPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 35f);

            Image image = _religionElementPrefab.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
            image.type = Image.Type.Sliced;

            GameObject bannerObject =
                Instantiate(
                    WindowPreloader.getWindowPrefab("kingdom")
                        .transform.FindRecursive("PrefabBannerReligion")
                        .gameObject, _religionElementPrefab.transform);
            bannerObject.name = "Banner";
            bannerObject.transform.localPosition = new Vector3(-85f, 0);

            GameObject textObject = new GameObject("Text", typeof(Text));
            textObject.transform.SetParent(_religionElementPrefab.transform);
            textObject.transform.localPosition = new Vector3(30f, -3.5f);
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 25f);
            Text text = textObject.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 16;
            text.supportRichText = true;

            _religionElementPrefab.SetActive(false);
        }

        internal class ReligionVisualElement : MonoBehaviour {
            private Religion _religion;

            public void SetReligion(Religion religion) {
                _religion = religion;

                Text text = transform.Find("Text").GetComponent<Text>();
                text.color = religion.getColor().getColorText();
                text.text = religion.name;

                transform.Find("Banner").GetComponent<ReligionBanner>().load(religion);
            }

            private void Awake() {
                gameObject.GetComponent<Button>().onClick.AddListener(() => { LastSelectedReligion = _religion; });
            }
        }
    }
}