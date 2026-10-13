using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ET_Homunculus
{
    /// <summary>
    /// Player-facing toggles for the cosmetic armour overlays, including the per-race
    /// show/hide switches (the "hidden clothing" feature).
    /// </summary>
    public class HomunculusSettings : ModSettings
    {
        /// <summary>Layer key -&gt; enabled. A missing key means enabled.</summary>
        private readonly Dictionary<string, bool> layerEnabled = new Dictionary<string, bool>();

        /// <summary>Layer key -&gt; race defName -&gt; enabled. A missing entry means enabled.</summary>
        private readonly Dictionary<string, Dictionary<string, bool>> raceEnabled =
            new Dictionary<string, Dictionary<string, bool>>();

        public bool IsLayerEnabled(string layerKey)
        {
            if (layerKey != null && layerEnabled.TryGetValue(layerKey, out bool v))
            {
                return v;
            }
            return true;
        }

        public bool IsLayerEnabled(string layerKey, string raceDefName)
        {
            if (!IsLayerEnabled(layerKey))
            {
                return false;
            }
            if (layerKey != null && raceDefName != null
                && raceEnabled.TryGetValue(layerKey, out var races)
                && races.TryGetValue(raceDefName, out bool v))
            {
                return v;
            }
            return true;
        }

        public void SetLayer(string layerKey, bool value)
        {
            layerEnabled[layerKey] = value;
        }

        public void SetRace(string layerKey, string raceDefName, bool value)
        {
            if (!raceEnabled.TryGetValue(layerKey, out var races))
            {
                races = new Dictionary<string, bool>();
                raceEnabled[layerKey] = races;
            }
            races[raceDefName] = value;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            var keys = new List<string>();
            var vals = new List<bool>();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                // Nested dictionaries are not scribable directly; flatten to two
                // parallel lists and rebuild on load.
                foreach (var layer in HomunculusOverlayLayers.All)
                {
                    keys.Add(layer.Key);
                    vals.Add(IsLayerEnabled(layer.Key));
                }
                foreach (var layer in HomunculusOverlayLayers.All)
                {
                    foreach (string race in HomunculusRaceDefs.All)
                    {
                        keys.Add(layer.Key + "|" + race);
                        vals.Add(IsLayerEnabled(layer.Key, race));
                    }
                }
            }

            Scribe_Collections.Look(ref keys, "overlayKeys", LookMode.Value);
            Scribe_Collections.Look(ref vals, "overlayValues", LookMode.Value);

            if (Scribe.mode != LoadSaveMode.LoadingVars || keys.NullOrEmpty() || keys.Count != vals.Count)
            {
                return;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                if (key == null)
                {
                    continue;
                }
                int bar = key.IndexOf('|');
                if (bar < 0)
                {
                    layerEnabled[key] = vals[i];
                }
                else
                {
                    SetRace(key.Substring(0, bar), key.Substring(bar + 1), vals[i]);
                }
            }
        }
    }
}
