using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    // Selector window: pick a Subspecies, then the Subspecies Magnet will only pick up units of that subspecies.
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

            // hide unused pooled elements
            for (int i = elementIndex; i < _elements.Count; i++) {
                _elements[i].SetActive(false);
            }
        }

        protected override void Init() {
            _vertLayoutGroup = this.BeginVertGroup();

            // Ensure the power button exists in NML's registry for window interactions.
            _magnetButton = PowerButtonCreator.CreateGodPowerButton("subspecies_magnet",
                SpriteTextureLoader.getSprite("ui/icons/subspecies_magnet_icon"));

            _elementPrefab = new GameObject("subspeciesElementPrefab", typeof(Image), typeof(Button),
                typeof(SubspeciesVisualElement));
            _elements = new List<GameObject>();

            _elementPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 35f);

            Image image = _elementPrefab.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
            image.type = Image.Type.Sliced;

            // Icon
            GameObject iconObject = new GameObject("Icon", typeof(Image));
            iconObject.transform.SetParent(_elementPrefab.transform);
            iconObject.transform.localPosition = new Vector3(-85f, 0f);
            iconObject.GetComponent<RectTransform>().sizeDelta = new Vector2(26f, 26f);

            // Text
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

                // Text: try to show a readable subspecies name (localized when possible).
                Text text = transform.Find("Text").GetComponent<Text>();

                string displayName = subspecies.name;
                text.text = string.IsNullOrEmpty(displayName) ? $"Subspecies {subspecies.id}" : displayName;

                // Color: attempt to match subspecies color for quick recognition.
                text.color = TryGetSubspeciesTextColor(subspecies) ?? Color.white;

                // Icon: best effort
                Image icon = transform.Find("Icon").GetComponent<Image>();
                icon.enabled = true;
                icon.sprite = SpriteTextureLoader.getSprite("ui/icons/iconSpecies");
            }

            private static string LocalizeIfPossible(string maybeKey) {
                if (string.IsNullOrEmpty(maybeKey)) {
                    return maybeKey;
                }

                try {
                    string localized = LocalizedTextManager.getText(maybeKey);

                    if (!string.IsNullOrEmpty(localized) && localized != maybeKey) {
                        return localized;
                    }
                } catch {
                    // ignore
                }

                return maybeKey;
            }

            private static Color? TryGetSubspeciesTextColor(Subspecies subspecies) {
                try {
                    // Preferred: subspecies.getColor().getColorText() (matches Culture behavior)
                    MethodInfo mGetColor = AccessTools.Method(subspecies.GetType(), "getColor");

                    if (mGetColor != null) {
                        object colorObj = mGetColor.Invoke(subspecies, null);

                        if (colorObj != null) {
                            MethodInfo mGetColorText = AccessTools.Method(colorObj.GetType(), "getColorText");

                            if (mGetColorText != null) {
                                object c = mGetColorText.Invoke(colorObj, null);

                                if (c is Color unityColor) {
                                    return unityColor;
                                }
                            }

                            // Sometimes getColor() returns a UnityEngine.Color directly
                            if (colorObj is Color directColor) {
                                return directColor;
                            }
                        }
                    }

                    // Fallback: a field named "color" (either Color or a wrapper with getColorText)
                    FieldInfo fColor = AccessTools.Field(subspecies.GetType(), "color");

                    if (fColor != null) {
                        object colorObj = fColor.GetValue(subspecies);

                        if (colorObj is Color unityColor) {
                            return unityColor;
                        }

                        if (colorObj != null) {
                            MethodInfo mGetColorText = AccessTools.Method(colorObj.GetType(), "getColorText");

                            if (mGetColorText != null) {
                                object c = mGetColorText.Invoke(colorObj, null);

                                if (c is Color unityColor2) {
                                    return unityColor2;
                                }
                            }
                        }
                    }
                } catch {
                    // ignore
                }

                return null;
            }

            private void Awake() {
                gameObject.GetComponent<Button>().onClick.AddListener(() => { LastSelectedSubspecies = _subspecies; });
            }
        }
    }
}