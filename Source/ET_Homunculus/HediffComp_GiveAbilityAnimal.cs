using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace ET_Homunculus
{
    public class HediffCompProperties_GiveAbilityAnimal : HediffCompProperties
    {
        public List<AbilityDef> abilityDefs;
        public HediffCompProperties_GiveAbilityAnimal() { compClass = typeof(HediffComp_GiveAbilityAnimal); }
    }

    public class HediffComp_GiveAbilityAnimal : HediffComp
    {
        private static FieldInfo abilitiesField;
        private bool abilitiesAdded;

        public HediffCompProperties_GiveAbilityAnimal Props => (HediffCompProperties_GiveAbilityAnimal)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (Pawn == null || Props.abilityDefs == null || abilitiesAdded) return;
            AddAbilities();
        }

        private void AddAbilities()
        {
            if (abilitiesField == null)
            {
                abilitiesField = typeof(Pawn).GetField("abilities",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (abilitiesField == null)
                {
                    Log.Error("[ET_Homunculus] 'abilities' field not found on Pawn. Cannot grant abilities.");
                    return;
                }
            }

            var tracker = abilitiesField.GetValue(Pawn);
            if (tracker == null)
            {
                var ctor = abilitiesField.FieldType.GetConstructor(new[] { typeof(Pawn) });
                if (ctor == null)
                {
                    Log.Error("[ET_Homunculus] AbilityTracker constructor not found.");
                    return;
                }
                tracker = ctor.Invoke(new object[] { Pawn });
                abilitiesField.SetValue(Pawn, tracker);
            }

            // Use reflection to call the AddAbility method
            // Try common method names for adding abilities
            var addMethod = tracker.GetType().GetMethod("GainAbility",
                BindingFlags.Public | BindingFlags.Instance);
            if (addMethod == null)
                addMethod = tracker.GetType().GetMethod("AddAbility",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (addMethod == null)
            {
                // List all methods for debugging
                var methods = tracker.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var names = string.Join(", ", methods.Select(m => m.Name).ToArray());
                Log.Error($"[ET_Homunculus] No GainAbility/AddAbility. Available methods: {names}");
                return;
            }

            foreach (var def in Props.abilityDefs)
            {
                try { addMethod.Invoke(tracker, new object[] { def }); }
                catch (System.Exception ex) { Log.Error($"[ET_Homunculus] Failed: {ex}"); }
            }

            abilitiesAdded = true;
            Log.Message($"[ET_Homunculus] Granted {Props.abilityDefs.Count} abilities to {Pawn.LabelShort}");
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref abilitiesAdded, "abilitiesAdded");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && abilitiesAdded && Pawn != null)
                abilitiesAdded = false; // trigger re-add
        }
    }
}
