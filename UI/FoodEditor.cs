using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.General.UI.Window.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class FoodEditor : AutoLayoutWindow<FoodEditor> {
        private static ResourceAsset _selectedResource;
        private AutoGridLayoutGroup _grid;
        private Image _iconImage;
        private InputField _inputField;
        private List<string> _loadedResources;
        private AutoVertLayoutGroup _vert;

        public override void OnNormalEnable() {
            UpdateButtons();
        }

        // Part of the input is pulled from NML
        protected override void Init() {
            _vert = this.BeginVertGroup();
            _grid = this.BeginGridGroup(6, pCellSize: new Vector2(32, 32));
            _loadedResources = new List<string>();

            GameObject inputObject = new GameObject("Input", typeof(Image), typeof(LayoutElement));
            GameObject inputFieldObject = new GameObject("InputField", typeof(Text), typeof(InputField));
            GameObject resIconObject = new GameObject("Icon", typeof(Image));
            GameObject iconObject = new GameObject("Image", typeof(Image));
            RectTransform inputFieldRect = inputFieldObject.GetComponent<RectTransform>();
            LayoutElement inputLayoutElement = inputObject.GetComponent<LayoutElement>();
            Image inputBg = inputObject.GetComponent<Image>();
            Text inputText = inputFieldObject.GetComponent<Text>();

            _inputField = inputFieldObject.GetComponent<InputField>();
            _iconImage = resIconObject.GetComponent<Image>();

            inputObject.transform.localScale = Vector3.one;
            inputObject.transform.localPosition = new Vector3(5f, 0f);
            inputFieldObject.transform.SetParent(inputObject.transform);
            inputFieldObject.transform.localPosition = Vector3.zero;
            inputFieldObject.transform.localScale = Vector3.one;

            inputFieldRect.sizeDelta = new Vector2(170f, 15f);
            inputObject.GetComponent<RectTransform>().sizeDelta = inputFieldRect.sizeDelta + new Vector2(2f, 2f);

            inputLayoutElement.preferredHeight = 25;

            inputBg.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
            inputBg.type = Image.Type.Sliced;

            resIconObject.transform.SetParent(inputObject.transform);
            resIconObject.transform.localPosition = new Vector3(-98f, 0f);
            resIconObject.transform.localScale = Vector3.one;
            resIconObject.GetComponent<RectTransform>().sizeDelta = new Vector2(28f, 28f);
            _iconImage.sprite = Resources.Load<Sprite>("ui/icons/icon_select_finger");

            iconObject.transform.SetParent(inputObject.transform);
            iconObject.transform.localPosition = new Vector3(77f, 0f);
            iconObject.transform.localScale = Vector3.one;
            iconObject.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/inputFieldIcon");
            iconObject.GetComponent<RectTransform>().sizeDelta = new Vector2(15f, 15f);

            inputText.font = LocalizedTextManager.current_font;
            inputText.fontSize = 10;
            inputText.supportRichText = true;
            inputText.alignment = TextAnchor.MiddleLeft;
            OT.InitializeCommonText(inputText);

            _inputField.textComponent = inputText;
            _inputField.text = LocalizedTextManager.getText("food_editor_input_default");
            _inputField.onValidateInput += ValidateInput;
            _inputField.onValueChanged.AddListener(UpdateResources);
            _inputField.interactable = false;

            _vert.layout.childControlHeight = true;

            _vert.AddChild(_grid.gameObject);
            _vert.AddChild(inputObject);

            UpdateButtons();
        }

        private static void UpdateResources(string text) {
            if (SelectedMetas.selected_city == null) {
                return;
            }

            bool isNumber = int.TryParse(text, out int afterValue);

            if (!isNumber) {
                return;
            }

            int difference = afterValue - SelectedMetas.selected_city.getResourcesAmount(_selectedResource.id);

            if (difference > 0) {
                SelectedMetas.selected_city.addResourcesToRandomStockpile(_selectedResource.id, difference);
            }

            if (difference < 0) {
                SelectedMetas.selected_city.takeResource(_selectedResource.id, -difference);
            }
        }

        private static char ValidateInput(string text, int charIndex, char addedChar) {
            return char.IsDigit(addedChar) && text.Length < 10 ? addedChar : '\0';
        }

        private void SetTextWithoutEvent(string input) {
            _inputField.onValueChanged.RemoveAllListeners();
            _inputField.onValidateInput -= ValidateInput;

            _inputField.text = input;

            _inputField.onValueChanged.AddListener(UpdateResources);
            _inputField.onValidateInput += ValidateInput;
        }

        private void UpdateButtons() {
            foreach (ResourceAsset resourceAsset in AssetManager.resources.list) {
                if (!resourceAsset.food) {
                    continue;
                }

                if (_loadedResources.Contains(resourceAsset.id)) {
                    continue;
                }

                _grid.AddChild(PowerButtonCreator.CreateSimpleButton($"edit_{resourceAsset.id}", () => {
                        _selectedResource = resourceAsset;
                        _inputField.interactable = true;
                        _iconImage.sprite = resourceAsset.getSpriteIcon();
                        SetTextWithoutEvent(SelectedMetas.selected_city.getResourcesAmount(resourceAsset.id)
                            .ToString());
                    }, resourceAsset.getSpriteIcon())
                    .gameObject);
                _loadedResources.Add(resourceAsset.id);
            }
        }
    }
}