using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI {
    internal class ForcedGeneInput : MonoBehaviour {
        private static readonly string[] _statIds = {
            "i_offspring", "i_mutation_rate", "i_lifespan_male", "i_lifespan_female", "i_maturation", "i_mana",
            "i_birth_rate", "i_health", "i_armor", "i_speed", "i_damage", "i_critical_chance", "i_attack_speed",
            "i_diplomacy", "i_warfare", "i_stewardship", "i_intelligence", "i_stamina"
        };

        private static readonly Dictionary<int, Dictionary<string, int>> LockedStats =
            new Dictionary<int, Dictionary<string, int>>();

        private static int _beforeValue;
        private static Image _iconImage;
        private static bool _initialized;
        private static InputField _inputField;
        private static StatsIcon _selectedStat;
        private static GameObject _unlockButton;
        private static GameObject _unlockButtonPlaceholder;

        public void SelectStat(GameObject statsButton) {
            StatsIcon statsIcon = statsButton.GetComponent<StatsIcon>();

            if (statsButton.name == "i_mana") {
                _iconImage.sprite = statsIcon.getIcon().sprite;
                SetTextWithoutEvent(LocalizedTextManager.getText("forced_gene_no_mana"));
                _inputField.interactable = false;
                _unlockButton.SetActive(false);
                _unlockButtonPlaceholder.SetActive(true);

                return;
            }

            _selectedStat = statsIcon;
            _beforeValue = statsButton.name.Contains("lifespan")
                ? (int) (statsIcon._value - SelectedMetas.selected_subspecies.base_stats["lifespan"])
                : (int) statsIcon._value;

            SetTextWithoutEvent(_beforeValue.ToString());
            _inputField.interactable = true;
            _iconImage.sprite = statsIcon.getIcon().sprite;

            int hashCode = SelectedMetas.selected_subspecies.nucleus.GetHashCode();
            bool showUnlockButton = LockedStats.ContainsKey(hashCode)
                                    && LockedStats[hashCode].ContainsKey(_selectedStat.name);

            _unlockButton.SetActive(showUnlockButton);
            _unlockButtonPlaceholder.SetActive(!showUnlockButton);
        }

        [HarmonyPatch(typeof(ScrollWindow), nameof(ScrollWindow.clickShow))]
        [HarmonyPostfix]
        private static void ClickShow_Postfix(bool pSkipAnimation, bool pJustCreated, ScrollWindow __instance) {
            if (_initialized && __instance.name == "subspecies") {
                ResetSelection();
            }
        }

        [HarmonyPatch(typeof(MapBox), nameof(MapBox.finishMakingWorld))]
        [HarmonyPostfix]
        private static void FinishMakingWorld_Postfix() {
            LockedStats.Clear();

            foreach (Subspecies subspecies in World.world.subspecies) {
                if (subspecies.data.custom_data_int == null) {
                    continue;
                }

                foreach (string statId in _statIds) {
                    if (subspecies.data.custom_data_int.TryGetValue(statId, out int value)) {
                        int hashCode = subspecies.nucleus.GetHashCode();

                        if (!LockedStats.ContainsKey(hashCode)) {
                            LockedStats.Add(hashCode, new Dictionary<string, int>());
                        }

                        LockedStats[hashCode][statId] = value;
                        subspecies.nucleus.recalculate();
                        subspecies.recalcBaseStats();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Nucleus), nameof(Nucleus.recalculate))]
        [HarmonyPostfix]
        private static void Recalculate_Postfix(Nucleus __instance) {
            int hashCode = __instance.GetHashCode();

            if (!LockedStats.TryGetValue(hashCode, out Dictionary<string, int> lockedStats)) {
                return;
            }

            foreach (KeyValuePair<string, int> lockedStat in lockedStats) {
                if (lockedStat.Key == "i_lifespan_male") {
                    __instance._merged_base_stats_male["lifespan"] = lockedStat.Value;

                    continue;
                }

                if (lockedStat.Key == "i_lifespan_female") {
                    __instance._merged_base_stats_female["lifespan"] = lockedStat.Value;

                    continue;
                }

                string statId = lockedStat.Key.Replace("i_", "").Replace("mutation_rate", "mutation");

                __instance._merged_base_stats[statId] = lockedStat.Value;
                __instance._merged_base_stats_meta[statId] = lockedStat.Value;
            }
        }

        private static void ResetSelection() {
            if (_iconImage == null
                || _inputField == null
                || _unlockButton == null
                || _unlockButtonPlaceholder == null) {
                return;
            }

            _beforeValue = 0;
            _selectedStat = null;
            _iconImage.sprite = Resources.Load<Sprite>("ui/icons/icon_select_finger");
            SetTextWithoutEvent(LocalizedTextManager.getText("forced_gene_input_default"));
            _inputField.interactable = false;
            _unlockButton.SetActive(false);
            _unlockButtonPlaceholder.SetActive(true);
        }

        private static void SetTextWithoutEvent(string input) {
            _inputField.onValueChanged.RemoveAllListeners();
            _inputField.onValidateInput -= ValidateInput;

            _inputField.text = input;

            _inputField.onValueChanged.AddListener(UpdateLockedStats);
            _inputField.onValidateInput += ValidateInput;
        }

        private static void UnlockSelected() {
            int hashCode = SelectedMetas.selected_subspecies.nucleus.GetHashCode();

            if (!LockedStats.TryGetValue(hashCode, out Dictionary<string, int> stat)) {
                return;
            }

            stat.Remove(_selectedStat.name);
            SelectedMetas.selected_subspecies.nucleus.setDirty();
            SelectedMetas.selected_subspecies.nucleus.recalculate();
            SelectedMetas.selected_subspecies.recalcBaseStats();

            if (_selectedStat.name == "i_lifespan_male") {
                _selectedStat.setValue(SelectedMetas.selected_subspecies.base_stats_male["lifespan"]
                                       + SelectedMetas.selected_subspecies.base_stats["lifespan"]);
            } else if (_selectedStat.name == "i_lifespan_female") {
                _selectedStat.setValue(SelectedMetas.selected_subspecies.nucleus._merged_base_stats_female["lifespan"]
                                       + SelectedMetas.selected_subspecies.base_stats["lifespan"]);
            } else {
                string statId = _selectedStat.name.Replace("i_", "").Replace("mutation_rate", "mutation");

                _selectedStat.setValue(SelectedMetas.selected_subspecies.nucleus._merged_base_stats[statId]);
            }

            ResetSelection();
        }

        private static void UpdateLockedStats(string text) {
            if (SelectedMetas.selected_subspecies == null) {
                return;
            }

            int hashCode = SelectedMetas.selected_subspecies.nucleus.GetHashCode();
            bool isNumber = int.TryParse(text, out int afterValue);

            if (!isNumber) {
                return;
            }

            if (!LockedStats.ContainsKey(hashCode)) {
                LockedStats.Add(hashCode, new Dictionary<string, int>());
            }

            LockedStats[hashCode][_selectedStat.name] = afterValue;
            SelectedMetas.selected_subspecies.data.set(_selectedStat.name, afterValue);
            SelectedMetas.selected_subspecies.nucleus.recalculate();
            SelectedMetas.selected_subspecies.recalcBaseStats();

            if (_selectedStat.name.Contains("lifespan")) {
                afterValue += (int) SelectedMetas.selected_subspecies.base_stats["lifespan"];
            }

            _selectedStat.setValue(afterValue);
            _unlockButton.SetActive(true);
            _unlockButtonPlaceholder.SetActive(false);
        }

        private static char ValidateInput(string text, int charIndex, char addedChar) {
            return char.IsDigit(addedChar) && text.Length < 10 ? addedChar : '\0';
        }

        private void Awake() {
            GameObject icon = new GameObject("Icon");
            GameObject input = new GameObject("Input");
            GameObject unlockButton = new GameObject("Unlock");
            GameObject unlockText = new GameObject("Text");
            GameObject unlockPlaceholder = new GameObject("Placeholder");

            HorizontalLayoutGroup layout = this.AddComponent<HorizontalLayoutGroup>();
            Image mainBackground = this.AddComponent<Image>();
            Image buttonBackground = unlockButton.AddComponent<Image>();
            Button button = unlockButton.AddComponent<Button>();
            Text unlockTextText = unlockText.AddComponent<Text>();
            Text inputText = input.AddComponent<Text>();
            _inputField = input.AddComponent<InputField>();
            _iconImage = icon.AddComponent<Image>();

            RectTransform iconTransform = icon.GetComponent<RectTransform>();
            RectTransform inputTransform = input.GetComponent<RectTransform>();
            RectTransform unlockButtonTransform = unlockButton.GetComponent<RectTransform>();
            RectTransform unlockTextTransform = unlockText.GetComponent<RectTransform>();
            RectTransform unlockPlaceholderTransform = unlockPlaceholder.AddComponent<RectTransform>();

            iconTransform.SetParent(transform);
            inputTransform.SetParent(transform);
            unlockButtonTransform.SetParent(transform);
            unlockTextTransform.SetParent(unlockButton.transform);
            unlockPlaceholderTransform.SetParent(transform);

            GetComponent<RectTransform>().sizeDelta = new Vector2(214f, 0f);

            iconTransform.sizeDelta = new Vector2(28f, 28f);
            inputTransform.sizeDelta = new Vector2(150f, 28f);
            unlockButtonTransform.sizeDelta = new Vector2(34f, 22f);
            unlockTextTransform.sizeDelta = new Vector2(34f, 22f);
            unlockPlaceholderTransform.sizeDelta = unlockButtonTransform.sizeDelta;

            iconTransform.localScale = Vector3.one;
            inputTransform.localScale = Vector3.one;
            unlockButtonTransform.localScale = Vector3.one;
            unlockTextTransform.localScale = Vector3.one;
            unlockPlaceholderTransform.localScale = Vector3.one;

            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(0, 4, 0, 0);
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            mainBackground.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
            mainBackground.type = Image.Type.Sliced;

            buttonBackground.sprite = Resources.Load<Sprite>("ui/special/special_buttonRed");

            button.onClick.AddListener(UnlockSelected);

            unlockTextText.font = LocalizedTextManager.current_font;
            unlockTextText.fontSize = 8;
            unlockTextText.supportRichText = true;
            unlockTextText.alignment = TextAnchor.MiddleCenter;
            unlockTextText.text = LocalizedTextManager.getText("forced_gene_reset");

            inputText.font = LocalizedTextManager.current_font;
            inputText.fontSize = 10;
            inputText.supportRichText = true;
            inputText.alignment = TextAnchor.MiddleLeft;

            _inputField.textComponent = inputText;
            _inputField.text = LocalizedTextManager.getText("forced_gene_input_default");
            _inputField.onValidateInput += ValidateInput;
            _inputField.onValueChanged.AddListener(UpdateLockedStats);
            _inputField.interactable = false;

            _iconImage.sprite = Resources.Load<Sprite>("ui/icons/icon_select_finger");

            _unlockButton = unlockButton;
            _unlockButtonPlaceholder = unlockPlaceholder;
            _unlockButton.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());

            _initialized = true;
        }
    }
}