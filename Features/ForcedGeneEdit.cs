using HarmonyLib;
using Sandbox.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.Features {
    internal static class ForcedGeneEdit {
        private static ForcedGeneInput _forcedGeneInput;

        public static void Init() {
            Harmony.CreateAndPatchAll(typeof(ForcedGeneEdit));
            Harmony.CreateAndPatchAll(typeof(ForcedGeneInput));

            ScrollWindow subspeciesWindow = WindowPreloader.getWindowPrefab("subspecies");
            Transform contentContainer = subspeciesWindow.transform.FindRecursive("content_gene_editor");

            GameObject input = new GameObject("ForcedGeneInput");

            input.transform.SetParent(contentContainer);
            input.transform.SetSiblingIndex(2);

            _forcedGeneInput = input.AddComponent<ForcedGeneInput>();
        }

        [HarmonyPatch(typeof(StatsIcon), nameof(StatsIcon.Awake))]
        [HarmonyPostfix]
        private static void Awake(StatsIcon __instance) {
            if (__instance.gameObject.transform.parent.childCount != 18) {
                return;
            }

            __instance.gameObject.AddOrGetComponent<Button>()
                .onClick.AddListener(() => { _forcedGeneInput.SelectStat(__instance.gameObject); });
        }
    }
}