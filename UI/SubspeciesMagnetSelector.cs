using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class SubspeciesMagnetSelector : AutoLayoutWindow<SubspeciesMagnetSelector> {
        public static Subspecies LastSelectedSubspecies;

        private GameObject _elementPrefab;
        private List<GameObject> _elements;
        private PowerButton _magnetButton;
        private AutoVertLayoutGroup _vertLayoutGroup;

        public override void OnNormalDisable() {
            foreach (GameObject element in _elements) {
                element.SetActive(false);
            }
        }

        public override void OnNormalEnable() {
            int elementIndex = 0;

            foreach (Subspecies subspecies in World.world.subspecies) {
                if (elementIndex >= _elements.Count) {
                    GameObject element = Instantiate(_elementPrefab);

                    element.GetComponent<Button>()
                        .onClick.AddListener(() => {
                            ScrollWindow.getCurrentWindow().clickHide();
                            _magnetButton.clickButton();
                        });

                    _vertLayoutGroup.AddChild(element);
                    _elements.Add(element);
                }

                _elements[elementIndex].SetActive(true);
                _elements[elementIndex].GetComponent<SubspeciesVisualElement>().SetSubspecies(subspecies);
                elementIndex++;
            }

            for (int i = elementIndex; i < _elements.Count; i++) {
                _elements[i].SetActive(false);
            }
        }

        protected override void Init() {
            _vertLayoutGroup = this.BeginVertGroup();

            _magnetButton = PowerButtonCreator.CreateGodPowerButton("subspecies_magnet",
                SpriteTextureLoader.getSprite("ui/icons/subspecies_magnet_icon"));

            _elementPrefab = new GameObject("subspeciesElementPrefab", typeof(Image), typeof(Button),
                typeof(SubspeciesVisualElement));
            _elements = new List<GameObject>();

            _elementPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 35f);

            Image image = _elementPrefab.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
            image.type = Image.Type.Sliced;

            GameObject iconObject = new GameObject("Icon", typeof(Image));
            iconObject.transform.SetParent(_elementPrefab.transform);
            iconObject.transform.localPosition = new Vector3(-85f, 0f);
            iconObject.GetComponent<RectTransform>().sizeDelta = new Vector2(26f, 26f);

            GameObject textObject = new GameObject("Text", typeof(Text));
            textObject.transform.SetParent(_elementPrefab.transform);
            textObject.transform.localPosition = new Vector3(30f, -3.5f);
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 25f);
            Text text = textObject.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 16;
            text.supportRichText = true;

            _elementPrefab.SetActive(false);
        }

        internal class SubspeciesVisualElement : MonoBehaviour {
            private Subspecies _subspecies;

            public void SetSubspecies(Subspecies subspecies) {
                _subspecies = subspecies;

                Text text = transform.Find("Text").GetComponent<Text>();
                text.color = subspecies.getColor().getColorText();
                text.text = subspecies.name;

                Image icon = transform.Find("Icon").GetComponent<Image>();
                icon.enabled = true;
                icon.sprite = SpriteTextureLoader.getSprite("ui/icons/iconSpecies");
            }

            private void Awake() {
                gameObject.GetComponent<Button>().onClick.AddListener(() => { LastSelectedSubspecies = _subspecies; });
            }
        }
    }
}