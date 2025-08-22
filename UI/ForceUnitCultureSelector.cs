using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class ForceUnitCultureSelector : AutoLayoutWindow<ForceUnitCultureSelector> {
        public static Culture LastSelectedCulture;
        private GameObject _cultureElementPrefab;
        private List<GameObject> _cultureElements;
        private PowerButton _forceCultureButton;
        private AutoVertLayoutGroup _vertLayoutGroup;

        public override void OnNormalDisable() {
            foreach (GameObject cultureElement in _cultureElements) {
                cultureElement.SetActive(false);
            }
        }

        public override void OnNormalEnable() {
            int elementIndex = 0;

            foreach (Culture culture in World.world.cultures) {
                if (elementIndex >= _cultureElements.Count) {
                    GameObject cultureElement = Instantiate(_cultureElementPrefab);

                    cultureElement.GetComponent<Button>()
                        .onClick.AddListener(() => {
                            ScrollWindow.getCurrentWindow().clickHide();
                            _forceCultureButton.clickButton();
                        });

                    _vertLayoutGroup.AddChild(cultureElement);
                    _cultureElements.Add(cultureElement);
                }

                _cultureElements[elementIndex].SetActive(true);
                _cultureElements[elementIndex].GetComponent<CultureVisualElement>().SetCulture(culture);
                elementIndex++;
            }
        }

        protected override void Init() {
            _vertLayoutGroup = this.BeginVertGroup();
            _forceCultureButton = PowerButtonCreator.CreateGodPowerButton("force_unit_culture",
                SpriteTextureLoader.getSprite("ui/icons/force_culture_icon"));
            _cultureElementPrefab = new GameObject("cultureElementPrefab", typeof(Image), typeof(Button),
                typeof(CultureVisualElement));
            _cultureElements = new List<GameObject>();

            _cultureElementPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 25f);

            Image image = _cultureElementPrefab.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
            image.type = Image.Type.Sliced;

            GameObject bannerObject =
                Instantiate(
                    WindowPreloader.getWindowPrefab("city").transform.FindRecursive("PrefabBannerCulture").gameObject,
                    _cultureElementPrefab.transform);
            bannerObject.name = "Banner";
            bannerObject.transform.localPosition = new Vector3(-85f, 0);

            GameObject textObject = new GameObject("Text", typeof(Text));
            textObject.transform.SetParent(_cultureElementPrefab.transform);
            textObject.transform.localPosition = new Vector3(30f, -3.5f);
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 25f);
            Text text = textObject.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 16;
            text.supportRichText = true;

            _cultureElementPrefab.SetActive(false);
        }

        internal class CultureVisualElement : MonoBehaviour {
            private Culture _culture;

            public void SetCulture(Culture culture) {
                _culture = culture;

                Text text = transform.Find("Text").GetComponent<Text>();
                text.color = culture.getColor().getColorText();
                text.text = culture.name;

                transform.Find("Banner").GetComponent<CultureBanner>().load(culture);
            }

            private void Awake() {
                gameObject.GetComponent<Button>().onClick.AddListener(() => { LastSelectedCulture = _culture; });
            }
        }
    }
}