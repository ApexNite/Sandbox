using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class ForceCityKingdomSelector : AutoLayoutWindow<ForceCityKingdomSelector> {
        public static Kingdom LastSelectedKingdom;
        private PowerButton _forceKingdomButton;
        private GameObject _kingdomElementPrefab;
        private List<GameObject> _kingdomElements;
        private AutoVertLayoutGroup _vertLayoutGroup;

        public override void OnNormalDisable() {
            foreach (GameObject kingdomElement in _kingdomElements) {
                kingdomElement.SetActive(false);
            }
        }

        public override void OnNormalEnable() {
            int elementIndex = 0;

            foreach (Kingdom kingdom in World.world.kingdoms) {
                if (elementIndex >= _kingdomElements.Count) {
                    GameObject kingdomElement = Instantiate(_kingdomElementPrefab);

                    kingdomElement.GetComponent<Button>()
                        .onClick.AddListener(() => {
                            ScrollWindow.getCurrentWindow().clickHide();
                            _forceKingdomButton.clickButton();
                        });

                    _vertLayoutGroup.AddChild(kingdomElement);
                    _kingdomElements.Add(kingdomElement);
                }

                _kingdomElements[elementIndex].SetActive(true);
                _kingdomElements[elementIndex].GetComponent<KingdomVisualElement>().SetKingdom(kingdom);
                _kingdomElements[elementIndex]
                    .transform.Find("Banner")
                    .GetComponent<Button>()
                    .onClick.RemoveAllListeners();
                elementIndex++;
            }
        }

        protected override void Init() {
            _vertLayoutGroup = this.BeginVertGroup();
            _forceKingdomButton = PowerButtonCreator.CreateGodPowerButton("force_city_kingdom",
                SpriteTextureLoader.getSprite("ui/icons/force_unit_city_icon"));
            _kingdomElementPrefab = new GameObject("KingdomElementPrefab", typeof(Image), typeof(Button),
                typeof(KingdomVisualElement));
            _kingdomElements = new List<GameObject>();

            _kingdomElementPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 35f);

            Image image = _kingdomElementPrefab.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
            image.type = Image.Type.Sliced;

            GameObject bannerObject =
                Instantiate(
                    WindowPreloader.getWindowPrefab("kingdom").transform.FindRecursive("Main Banner").gameObject,
                    _kingdomElementPrefab.transform);
            bannerObject.name = "Banner";
            bannerObject.transform.localPosition = new Vector3(-85f, 0);
            bannerObject.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

            GameObject textObject = new GameObject("Text", typeof(Text));
            textObject.transform.SetParent(_kingdomElementPrefab.transform);
            textObject.transform.localPosition = new Vector3(30f, -3.5f);
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 25f);
            Text text = textObject.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 16;
            text.supportRichText = true;

            _kingdomElementPrefab.SetActive(false);
        }

        internal class KingdomVisualElement : MonoBehaviour {
            private Kingdom _kingdom;

            public void SetKingdom(Kingdom kingdom) {
                _kingdom = kingdom;

                Text text = transform.Find("Text").GetComponent<Text>();
                text.color = kingdom.getColor().getColorText();
                text.text = kingdom.name;

                transform.Find("Banner").GetComponent<KingdomBanner>().load(kingdom);
            }

            private void Awake() {
                gameObject.GetComponent<Button>().onClick.AddListener(() => { LastSelectedKingdom = _kingdom; });
            }
        }
    }
}