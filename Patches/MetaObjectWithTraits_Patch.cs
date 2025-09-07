using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Sandbox.UI;

namespace Sandbox.Patches {
    internal class MetaObjectWithTraits_Patch {
        private static bool _patched;

        public static bool DisableClanTraitsEnabled { get; set; }

        public static bool DisableCultureTraitsEnabled { get; set; }

        public static bool DisableLanguageTraitsEnabled { get; set; }

        public static bool DisableReligionTraitsEnabled { get; set; }

        public static void Patch() {
            if (_patched) {
                return;
            }

            Harmony harmony = new Harmony(nameof(MetaObjectWithTraits_Patch));
            Type originalClass =
                typeof(MetaObjectWithTraits<,>).MakeGenericType(typeof(MetaObjectData), typeof(GenericBaseTrait));
            Type patchClass = typeof(MetaObjectWithTraits_Patch);
            MethodInfo addTraitsMethod =
                originalClass.GetMethod("addTrait", new[] { typeof(GenericBaseTrait), typeof(bool) });
            MethodInfo fillTraitAssetsFromStringListMethod =
                AccessTools.Method(originalClass, "fillTraitAssetsFromStringList");
            MethodInfo addTraitsPrefix = AccessTools.Method(patchClass, nameof(AddTraits_Prefix));
            MethodInfo fillTraitAssetsFromStringListPrefix = AccessTools.Method(patchClass,
                nameof(FillTraitAssetsFromStringList_Prefix));

            harmony.CreateProcessor(addTraitsMethod).AddPrefix(addTraitsPrefix).Patch();
            harmony.CreateProcessor(fillTraitAssetsFromStringListMethod)
                .AddPrefix(fillTraitAssetsFromStringListPrefix)
                .Patch();

            _patched = true;
        }

        private static bool AddTraits_Prefix(GenericBaseTrait pTrait, bool pRemoveOpposites, ref bool __result) {
            if (pTrait.GetType() == typeof(ClanTrait) && DisableClanTraitsEnabled) {
                if (DisableClanTraitsWindow.ClanTraitEnabled(pTrait.id)) {
                    return true;
                }

                __result = false;

                return false;
            }

            if (pTrait.GetType() == typeof(CultureTrait) && DisableCultureTraitsEnabled) {
                if (DisableCultureTraitsWindow.CultureTraitEnabled(pTrait.id)) {
                    return true;
                }

                __result = false;

                return false;
            }

            if (pTrait.GetType() == typeof(LanguageTrait) && DisableLanguageTraitsEnabled) {
                if (DisableLanguageTraitsWindow.LanguageTraitEnabled(pTrait.id)) {
                    return true;
                }

                __result = false;

                return false;
            }

            if (pTrait.GetType() == typeof(ReligionTrait) && DisableReligionTraitsEnabled) {
                if (DisableReligionTraitsWindow.ReligionTraitEnabled(pTrait.id)) {
                    return true;
                }

                __result = false;

                return false;
            }

            return true;
        }

        private static bool FillTraitAssetsFromStringList_Prefix(ref List<string> pList,
            MetaObjectWithTraits<MetaObjectData, GenericBaseTrait> __instance) {
            if (__instance.meta_type == MetaType.Clan && DisableClanTraitsEnabled) {
                if (pList == __instance.default_traits) {
                    pList = pList.Where(DisableClanTraitsWindow.ClanTraitEnabled).ToList();
                }
            }

            if (__instance.meta_type == MetaType.Culture && DisableCultureTraitsEnabled) {
                if (pList == __instance.default_traits) {
                    pList = pList.Where(DisableCultureTraitsWindow.CultureTraitEnabled).ToList();
                }
            }

            if (__instance.meta_type == MetaType.Language && DisableLanguageTraitsEnabled) {
                if (pList == __instance.default_traits) {
                    pList = pList.Where(DisableLanguageTraitsWindow.LanguageTraitEnabled).ToList();
                }
            }

            if (__instance.meta_type == MetaType.Religion && DisableReligionTraitsEnabled) {
                if (pList == __instance.default_traits) {
                    pList = pList.Where(DisableReligionTraitsWindow.ReligionTraitEnabled).ToList();
                }
            }

            return true;
        }

        internal class GenericBaseTrait : BaseTrait<GenericBaseTrait> { }
    }
}