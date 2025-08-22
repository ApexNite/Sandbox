using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.Toolkit.Graphics {
    public class ButtonBuilder {
        public ButtonBuilder(string id, ButtonStyle style) {
            Next(id, style);
        }

        private string DescriptionKey { get; set; }

        private GodPower GodPower { get; set; }

        private Sprite Icon { get; set; }

        private string Id { get; set; }

        private Vector2 Scale { get; set; }

        private ButtonStyle Style { get; set; }

        private string TitleKey { get; set; }

        private PowerButtonType Type { get; set; }

        private string WindowId { get; set; }

        public ButtonBuilder Build(out PowerButton powerButton) {
            GameObject buttonObject = new GameObject(Id);
            GameObject iconObject = new GameObject("Icon");

            Image image = buttonObject.AddComponent<Image>();
            TipButton tipButton = buttonObject.AddComponent<TipButton>();
            powerButton = buttonObject.AddComponent<PowerButton>();
            Image icon = iconObject.AddComponent<Image>();
            buttonObject.AddComponent<Button>();

            iconObject.transform.SetParent(buttonObject.transform);

            buttonObject.GetComponent<RectTransform>().sizeDelta = Scale;
            iconObject.GetComponent<RectTransform>().sizeDelta = Scale - new Vector2(4f, 4f);
            buttonObject.transform.localScale = Vector3.one;
            iconObject.transform.localScale = Vector3.one;

            switch (Style) {
                case ButtonStyle.Small:
                    image.sprite = Resources.Load<Sprite>("ui/button");

                    break;
                case ButtonStyle.Medium:
                    image.sprite = Resources.Load<Sprite>("ui/buttonMedium");

                    break;
                case ButtonStyle.Long:
                    image.sprite = Resources.Load<Sprite>("ui/buttonLong");

                    break;
                case ButtonStyle.SpecialRed:
                    image.sprite = Resources.Load<Sprite>("ui/special/special_buttonRed");
                    image.type = Image.Type.Sliced;

                    break;
                case ButtonStyle.SpecialRedBorder:
                    image.sprite = Resources.Load<Sprite>("ui/special/special_buttonRed_insides");
                    image.type = Image.Type.Sliced;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            icon.sprite = Icon;

            tipButton.textOnClick = TitleKey;
            tipButton.textOnClickDescription = DescriptionKey;

            powerButton.type = Type;
            powerButton.icon = icon;

            switch (Type) {
                case PowerButtonType.Active:
                    powerButton.godPower = GodPower;

                    break;
                case PowerButtonType.Window:
                    powerButton.open_window_id = WindowId;

                    break;
                case PowerButtonType.Library:
                case PowerButtonType.Options:
                case PowerButtonType.BrushSize:
                case PowerButtonType.BrushSizeMain:
                case PowerButtonType.TimeScale:
                case PowerButtonType.Shop:
                case PowerButtonType.Special:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return this;
        }

        public ButtonBuilder Next(string id, ButtonStyle style) {
            DescriptionKey = $"{id}_description";
            Icon = Resources.Load<Sprite>("");
            Id = id;
            Scale = StyleToScale(style);
            Type = (PowerButtonType) (-1);
            Style = style;
            TitleKey = id;
            WindowId = string.Empty;

            return this;
        }

        public ButtonBuilder SetGodPower(string id) {
            Type = PowerButtonType.Active;
            GodPower = AssetManager.powers.get(id);

            return this;
        }

        public ButtonBuilder SetGodPower(GodPower godPower) {
            Type = PowerButtonType.Active;
            GodPower = godPower;

            return this;
        }

        public ButtonBuilder SetIcon(string path) {
            Icon = Resources.Load<Sprite>(path);

            return this;
        }

        public ButtonBuilder SetWindowId(string id) {
            _ = id ?? throw new ArgumentNullException(nameof(id));

            Type = PowerButtonType.Window;
            WindowId = id;

            return this;
        }

        private Vector2 StyleToScale(ButtonStyle style) {
            switch (style) {
                case ButtonStyle.Small:
                case ButtonStyle.SpecialRed:
                case ButtonStyle.SpecialRedBorder:
                    return new Vector2(32f, 32f);
                case ButtonStyle.Medium:
                    return new Vector2(89f, 32f);
                case ButtonStyle.Long:
                    return new Vector2(116f, 32f);
                default:
                    return Vector2.zero;
            }
        }
    }
}