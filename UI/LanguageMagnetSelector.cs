using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class LanguageMagnetSelector : AutoLayoutWindow<LanguageMagnetSelector> {
        public static Language LastSelectedLanguage;
        private GameObject _languageElementPrefab;
        private List<GameObject> _languageElements;
        private PowerButton _languageMagnetButton;
        private AutoVertLayoutGroup _vertLayoutGroup;

        public override void OnNormalDisable() {
            foreach (GameObject languageElement in _languageElements) {
                languageElement.SetActive(false);
            }
        }

        public override void OnNormalEnable() {
            int elementIndex = 0;

            foreach (Language language in World.world.languages) {
                if (elementIndex >= _languageElements.Count) {
                    GameObject languageElement = Instantiate(_languageElementPrefab);

                    languageElement.GetComponent<Button>()
                        .onClick.AddListener(() => {
                            ScrollWindow.getCurrentWindow().clickHide();
                            _languageMagnetButton.clickButton();
                        });

                    _vertLayoutGroup.AddChild(languageElement);
                    _languageElements.Add(languageElement);
                }

                _languageElements[elementIndex].SetActive(true);
                _languageElements[elementIndex].GetComponent<LanguageVisualElement>().SetLanguage(language);
                elementIndex++;
            }
        }

        protected override void Init() {
            _vertLayoutGroup = this.BeginVertGroup();
            _languageMagnetButton = PowerButtonCreator.CreateGodPowerButton("language_magnet",
                SpriteTextureLoader.getSprite("ui/icons/language_magnet_icon"));
            _languageElementPrefab = new GameObject("languageElementPrefab", typeof(Image), typeof(Button),
                typeof(LanguageVisualElement));
            _languageElements = new List<GameObject>();

            _languageElementPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 35f);

            Image image = _languageElementPrefab.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
            image.type = Image.Type.Sliced;

            GameObject bannerObject =
                Instantiate(
                    WindowPreloader.getWindowPrefab("kingdom")
                        .transform.FindRecursive("PrefabBannerLanguage")
                        .gameObject, _languageElementPrefab.transform);
            bannerObject.name = "Banner";
            bannerObject.transform.localPosition = new Vector3(-85f, 0);

            GameObject textObject = new GameObject("Text", typeof(Text));
            textObject.transform.SetParent(_languageElementPrefab.transform);
            textObject.transform.localPosition = new Vector3(30f, -3.5f);
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 25f);
            Text text = textObject.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 16;
            text.supportRichText = true;

            _languageElementPrefab.SetActive(false);
        }

        internal class LanguageVisualElement : MonoBehaviour {
            private Language _language;

            public void SetLanguage(Language language) {
                _language = language;

                Text text = transform.Find("Text").GetComponent<Text>();
                text.color = language.getColor().getColorText();
                text.text = language.name;

                transform.Find("Banner").GetComponent<LanguageBanner>().load(language);
            }

            private void Awake() {
                gameObject.GetComponent<Button>().onClick.AddListener(() => { LastSelectedLanguage = _language; });
            }
        }
    }
}