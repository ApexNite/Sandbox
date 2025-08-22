namespace Sandbox.Features {
    internal class ForceUnitPlot {
        public static void Init() {
            GodPower power = AssetManager.powers.clone("force_plot", "$template_drops$");
            DropAsset dropAsset = AssetManager.drops.add(new DropAsset {
                id = "force_plot",
                path_texture = "drops/drop_gamma_rain",
                random_frame = true,
                default_scale = 0.1f,
                action_landed = ForcePlot,
                material = "mat_world_object_lit",
                sound_drop = "event:/SFX/DROPS/DropRainGamma",
                type = DropType.DropGeneric
            });

            power.name = "force_plot";
            power.drop_id = "force_plot";
            power.cached_drop_asset = dropAsset;
        }

        private static void ForcePlot(WorldTile worldTile, string dropId) {
            foreach (Actor actor in Finder.getUnitsFromChunk(worldTile, 1, 3f)) {
                if (actor.hasPlot()) {
                    actor.plot.finishPlot(PlotState.Finished, actor);
                }
            }
        }
    }
}