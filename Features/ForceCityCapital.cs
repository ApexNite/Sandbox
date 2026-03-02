using System;
using System.Reflection;

namespace Sandbox.Features {
    internal class ForceCityCapital {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("force_capital", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "force_capital",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = ForceCapital,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "force_capital";
            power.drop_id = "force_capital";
            power.cached_drop_asset = dropAsset;
        }

        private static void ForceCapital(WorldTile worldTile, string dropId) {
            if (worldTile == null || !worldTile.hasCity()) return;
            City city = worldTile.zone_city;
            if (city == null || city.isRekt()) return;
            Kingdom kingdom = city.kingdom;
            if (kingdom == null) return;

            if (!TryInvokeCapitalSetter(kingdom, city)) {
                TrySetCapitalFields(kingdom, city);
            }

            TrySetCityCapitalFlags(city);
            TryInvokePostCapitalUpdate(kingdom);
        }

        private static bool TryInvokeCapitalSetter(Kingdom kingdom, City city) {
            Type t = kingdom.GetType();
            string[] names = {
                "setCapital","SetCapital","set_capital",
                "setCapitalCity","SetCapitalCity",
                "makeCapital","MakeCapital",
                "setCapitalTown","SetCapitalTown",
                "setCapitalCityObj","SetCapitalCityObj"
            };

            foreach (string n in names) {
                MethodInfo m = t.GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (m == null) continue;
                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType.IsAssignableFrom(city.GetType())) {
                    m.Invoke(kingdom, new object[] { city });
                    return true;
                }
            }

            return false;
        }

        private static void TrySetCapitalFields(Kingdom kingdom, City city) {
            Type t = kingdom.GetType();
            string[] fieldNames = { "capital", "capitalCity", "capital_city", "city_capital", "capitalTown", "_capital", "m_capital", "capital_city_obj" };
            foreach (string n in fieldNames) {
                FieldInfo f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null && f.FieldType.IsAssignableFrom(city.GetType())) {
                    f.SetValue(kingdom, city);
                    return;
                }
                PropertyInfo p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.CanWrite && p.PropertyType.IsAssignableFrom(city.GetType())) {
                    p.SetValue(kingdom, city, null);
                    return;
                }
            }
        }

        private static void TrySetCityCapitalFlags(City city) {
            Type t = city.GetType();
            string[] fieldNames = { "isCapital", "is_capital", "capital", "_capital", "m_isCapital", "m_capital" };
            foreach (string n in fieldNames) {
                FieldInfo f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null && f.FieldType == typeof(bool)) {
                    f.SetValue(city, true);
                }
                PropertyInfo p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.CanWrite && p.PropertyType == typeof(bool)) {
                    p.SetValue(city, true, null);
                }
            }

            MethodInfo mi = t.GetMethod("makeCapital", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                           ?? t.GetMethod("MakeCapital", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi != null && mi.GetParameters().Length == 0) {
                mi.Invoke(city, null);
            }
        }

        private static void TryInvokePostCapitalUpdate(Kingdom kingdom) {
            Type t = kingdom.GetType();
            string[] names = { "updateCapital", "UpdateCapital", "checkCapital", "CheckCapital", "updateKingdom", "UpdateKingdom", "updateCities", "UpdateCities" };
            foreach (string n in names) {
                MethodInfo m = t.GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (m == null) continue;
                if (m.GetParameters().Length == 0) {
                    m.Invoke(kingdom, null);
                    break;
                }
            }
        }
    }
}
