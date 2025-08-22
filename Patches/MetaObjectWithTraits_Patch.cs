using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

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
                if (!PlayerConfig.optionBoolEnabled($"{pTrait.id}_clan_trait_toggle")) {
                    __result = false;

                    return false;
                }

                return true;
            }

            if (pTrait.GetType() == typeof(CultureTrait) && DisableCultureTraitsEnabled) {
                if (!PlayerConfig.optionBoolEnabled($"{pTrait.id}_culture_trait_toggle")) {
                    __result = false;

                    return false;
                }

                return true;
            }

            if (pTrait.GetType() == typeof(LanguageTrait) && DisableLanguageTraitsEnabled) {
                if (!PlayerConfig.optionBoolEnabled($"{pTrait.id}_language_trait_toggle")) {
                    __result = false;

                    return false;
                }

                return true;
            }

            if (pTrait.GetType() == typeof(ReligionTrait) && DisableReligionTraitsEnabled) {
                if (!PlayerConfig.optionBoolEnabled($"{pTrait.id}_religion_trait_toggle")) {
                    __result = false;

                    return false;
                }

                return true;
            }

            return true;
        }

        private static bool FillTraitAssetsFromStringList_Prefix(ref List<string> pList,
            MetaObjectWithTraits<MetaObjectData, GenericBaseTrait> __instance) {
            if (__instance.meta_type == MetaType.Clan && DisableClanTraitsEnabled) {
                if (pList == __instance.default_traits) {
                    pList = pList.Where(id => PlayerConfig.optionBoolEnabled($"{id}_clan_trait_toggle")).ToList();
                }
            }

            if (__instance.meta_type == MetaType.Culture && DisableCultureTraitsEnabled) {
                if (pList == __instance.default_traits) {
                    pList = pList.Where(id => PlayerConfig.optionBoolEnabled($"{id}_culture_trait_toggle")).ToList();
                }
            }

            if (__instance.meta_type == MetaType.Language && DisableLanguageTraitsEnabled) {
                if (pList == __instance.default_traits) {
                    pList = pList.Where(id => PlayerConfig.optionBoolEnabled($"{id}_language_trait_toggle")).ToList();
                }
            }

            if (__instance.meta_type == MetaType.Religion && DisableReligionTraitsEnabled) {
                if (pList == __instance.default_traits) {
                    pList = pList.Where(id => PlayerConfig.optionBoolEnabled($"{id}_religion_trait_toggle")).ToList();
                }
            }

            return true;
        }

        internal class GenericBaseTrait : BaseTrait<GenericBaseTrait> { }
    }
}