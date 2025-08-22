using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class ForceUnitCitySelector : AutoLayoutWindow<ForceUnitCitySelector> {
        public static City LastSelectedCity;
        private GameObject _cityElementPrefab;
        private List<GameObject> _cityElements;
        private PowerButton _forceCityButton;
        private AutoVertLayoutGroup _vertLayoutGroup;

        public override void OnNormalDisable() {
            foreach (GameObject cityElement in _cityElements) {
                cityElement.SetActive(false);
            }
        }

        public override void OnNormalEnable() {
            int elementIndex = 0;

            foreach (City city in World.world.cities) {
                if (elementIndex >= _cityElements.Count) {
                    GameObject cityElement = Instantiate(_cityElementPrefab);

                    cityElement.GetComponent<Button>()
                        .onClick.AddListener(() => {
                            ScrollWindow.getCurrentWindow().clickHide();
                            _forceCityButton.clickButton();
                        });

                    _vertLayoutGroup.AddChild(cityElement);
                    _cityElements.Add(cityElement);
                }

                _cityElements[elementIndex].SetActive(true);
                _cityElements[elementIndex].GetComponent<CityVisualElement>().SetCity(city);
                elementIndex++;
            }
        }

        protected override void Init() {
            _vertLayoutGroup = this.BeginVertGroup();
            _forceCityButton = PowerButtonCreator.CreateGodPowerButton("force_unit_city",
                SpriteTextureLoader.getSprite("ui/icons/force_unit_city"));
            _cityElementPrefab = new GameObject("CityElementPrefab", typeof(Image), typeof(Button),
                typeof(CityVisualElement));
            _cityElements = new List<GameObject>();

            _cityElementPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 25f);

            Image image = _cityElementPrefab.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
            image.type = Image.Type.Sliced;

            GameObject speciesObject = new GameObject("Species", typeof(Image));
            speciesObject.transform.SetParent(_cityElementPrefab.transform);
            speciesObject.transform.localPosition = new Vector3(-85f, 0);
            speciesObject.GetComponent<RectTransform>().sizeDelta = new Vector2(25f, 25f);

            GameObject textObject = new GameObject("Text", typeof(Text));
            textObject.transform.SetParent(_cityElementPrefab.transform);
            textObject.transform.localPosition = new Vector3(30f, -3.5f);
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 25f);
            Text text = textObject.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 16;
            text.supportRichText = true;

            _cityElementPrefab.SetActive(false);
        }

        internal class CityVisualElement : MonoBehaviour {
            private City _city;

            public void SetCity(City city) {
                _city = city;

                Text text = transform.Find("Text").GetComponent<Text>();
                text.color = city.kingdom.kingdomColor.getColorText();
                text.text = city.name;

                transform.Find("Species").GetComponent<Image>().sprite = city.kingdom.getSpeciesIcon();
            }

            private void Awake() {
                gameObject.GetComponent<Button>().onClick.AddListener(() => { LastSelectedCity = _city; });
            }
        }
    }
}