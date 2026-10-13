using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ET_Homunculus
{
    /// <summary>
    /// Registers the mod settings page and owns the single settings instance that the
    /// overlay render nodes consult.
    ///
    /// RimWorld constructs exactly one Mod subclass per ModContentPack, so this is the
    /// safe place for shared state (Humanoid Dryad's DryadArmorMod uses the same idea).
    /// </summary>
    public class HomunculusMod : Mod
    {
        public static HomunculusMod Instance;
        public static HomunculusSettings Settings;

        public HomunculusMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<HomunculusSettings>();
        }

        /// <summary>
        /// Force every live pawn to rebuild its render tree, so an overlay toggle shows
        /// up at once rather than on the next natural recache.
        ///
        /// This deliberately walks the maps instead of keeping a registry of nodes: the
        /// base PawnRenderNode has no disposal hook, so a static node list would retain
        /// references to dead pawns and leak across map loads.
        /// </summary>
        public static void InvalidateAllNodes()
        {
            if (Find.Maps == null)
            {
                return;
            }
            for (int m = 0; m < Find.Maps.Count; m++)
            {
                Map map = Find.Maps[m];
                if (map?.mapPawns == null)
                {
                    continue;
                }
                IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < pawns.Count; i++)
                {
                    PawnRenderer renderer = pawns[i]?.Drawer?.renderer;
                    if (renderer == null)
                    {
                        continue;
                    }
                    // The tree rebuild re-runs GraphicFor (which re-reads the settings);
                    // SetAllGraphicsDirty re-derives the cached materials.
                    renderer.renderTree?.SetDirty();
                    renderer.SetAllGraphicsDirty();
                }
            }
        }

        /// <summary>
        /// Should this overlay layer be drawn for this race? Defaults to visible when the
        /// settings object does not exist yet, which can happen during very early load.
        /// </summary>
        public static bool IsLayerVisible(OverlayLayer layer, string raceDefName)
        {
            if (layer == null)
            {
                return false;
            }
            HomunculusSettings s = Settings;
            return s == null || s.IsLayerEnabled(layer.Key, raceDefName);
        }

        public override string SettingsCategory()
        {
            return "ET_Homunculus".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);

            list.Label("ET_OverlaySettingsHeader".Translate());
            list.GapLine();

            float lineHeight = Text.LineHeight;

            for (int i = 0; i < HomunculusOverlayLayers.All.Count; i++)
            {
                OverlayLayer layer = HomunculusOverlayLayers.All[i];

                bool layerVal = Settings.IsLayerEnabled(layer.Key);
                DrawCheckboxRow(list, 0f, ref layerVal, layer.LabelKey.Translate());
                Settings.SetLayer(layer.Key, layerVal);

                // Per-race show/hide switches (the "hidden clothing" toggles).
                for (int r = 0; r < HomunculusRaceDefs.All.Length; r++)
                {
                    string race = HomunculusRaceDefs.All[r];
                    bool raceVal = Settings.IsLayerEnabled(layer.Key, race);
                    DrawCheckboxRow(list, lineHeight + 8f, ref raceVal, HomunculusRaceDefs.LabelFor(race));
                    Settings.SetRace(layer.Key, race, raceVal);
                }

                list.Gap(6f);
            }

            list.Gap();
            list.Label("ET_OverlaySettingsFooter".Translate());
            list.End();
        }

        private static void DrawCheckboxRow(Listing_Standard list, float indent, ref bool val, string label)
        {
            float lineHeight = Text.LineHeight;
            float checkWidth = lineHeight + 4f;

            Rect row = list.GetRect(lineHeight);
            row.xMin += indent;

            var checkRect = new Rect(row.x, row.y, checkWidth, lineHeight);
            Widgets.Checkbox(checkRect.position, ref val, checkRect.width);

            var labelRect = new Rect(row.x + checkWidth + 4f, row.y, row.width - checkWidth - 4f, lineHeight);
            Widgets.Label(labelRect, label);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            InvalidateAllNodes();
        }
    }
}
