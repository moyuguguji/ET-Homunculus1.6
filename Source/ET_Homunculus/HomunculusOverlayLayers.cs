using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ET_Homunculus
{
    /// <summary>
    /// The homunculus races that can wear the cosmetic armour overlays.
    ///
    /// <see cref="All"/> holds ThingDef defNames, because the render node resolves its
    /// texture as "&lt;base texPath&gt;_&lt;pawn.def.defName&gt;". <see cref="LabelKey"/>
    /// maps a race to its translation key so the settings page never shows a raw
    /// defName.
    /// </summary>
    public static class HomunculusRaceDefs
    {
        public static readonly string[] All = { "Nymph", "Oread", "Naiad" };

        public static string LabelKeyFor(string raceDefName)
        {
            switch (raceDefName)
            {
                case "Nymph": return "ET_Race_Nymph";
                case "Oread": return "ET_Race_Oread";
                case "Naiad": return "ET_Race_Naiad";
                default: return raceDefName;
            }
        }

        /// <summary>Display name for the settings page; falls back to the defName.</summary>
        public static string LabelFor(string raceDefName)
        {
            string key = LabelKeyFor(raceDefName);
            string translated = key.Translate();
            // Translator returns the key itself when it is missing.
            return translated == key ? raceDefName : translated;
        }

        public static bool IsHomunculus(Pawn pawn)
        {
            return pawn?.def != null && All.Contains(pawn.def.defName);
        }
    }

    /// <summary>
    /// One toggleable "hidden clothing" layer: a body overlay texture shown only while
    /// the matching implant hediff is installed, and only when the player has switched
    /// that layer (globally, or for that specific race) on in the mod settings.
    ///
    /// Modelled on Humanoid Dryad's DryadArmorConfig, but keyed by hediff rather than
    /// by pawn kind, because ET.Homunculus armour is an implant rather than a caste.
    /// </summary>
    public class OverlayLayer
    {
        /// <summary>Settings key. Stable; renaming it silently resets the player's choice.</summary>
        public readonly string Key;

        /// <summary>Translation key for the settings checkbox label.</summary>
        public readonly string LabelKey;

        /// <summary>Implant hediff that must be present for this layer to be drawn.</summary>
        public readonly string HediffDefName;

        private HediffDef hediffInt;

        public OverlayLayer(string key, string labelKey, string hediffDefName)
        {
            Key = key;
            LabelKey = labelKey;
            HediffDefName = hediffDefName;
        }

        /// <summary>Resolved lazily: no defs exist yet when the static list is built.</summary>
        public HediffDef Hediff
        {
            get
            {
                if (hediffInt == null)
                {
                    hediffInt = DefDatabase<HediffDef>.GetNamedSilentFail(HediffDefName);
                }
                return hediffInt;
            }
        }
    }

    /// <summary>
    /// The three ET.Homunculus armour tiers that carry a body overlay.
    ///
    ///  - HO_AT_Frame   / HObasearmor -> 框架衣物
    ///  - HO_AT_Armor   / HOarmor     -> 护甲衣物
    ///  - HO_AT_ArmorAD / HOarmorAD   -> 护甲二衣物
    /// </summary>
    public static class HomunculusOverlayLayers
    {
        public const string KeyFrame = "ET_Overlay_Frame";
        public const string KeyArmor = "ET_Overlay_Armor";
        public const string KeyArmorAD = "ET_Overlay_ArmorAD";

        public static readonly List<OverlayLayer> All = new List<OverlayLayer>
        {
            new OverlayLayer(KeyFrame,   "ET_Overlay_Frame",   "HO_AT_Frame"),
            new OverlayLayer(KeyArmor,   "ET_Overlay_Armor",   "HO_AT_Armor"),
            new OverlayLayer(KeyArmorAD, "ET_Overlay_ArmorAD", "HO_AT_ArmorAD"),
        };

        /// <summary>The layer driven by a given hediff, or null when it has no overlay.</summary>
        public static OverlayLayer ForHediff(HediffDef def)
        {
            if (def == null)
            {
                return null;
            }
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].HediffDefName == def.defName)
                {
                    return All[i];
                }
            }
            return null;
        }
    }
}
