using System.Linq;
using RimWorld;
using Verse;

namespace ET_Homunculus
{
    public class CompProperties_AbilityHeal : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityHeal() { compClass = typeof(CompAbilityEffect_Heal); }
    }

    public class CompAbilityEffect_Heal : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var pawn = target.Pawn;
            if (pawn == null) return;

            var candidates = pawn.health.hediffSet.hediffs
                .Where(h => h.IsPermanent() || (h.def.isBad && h.def.everCurableByItem))
                .ToList();

            if (candidates.Count > 0)
            {
                var toHeal = candidates.RandomElement();
                pawn.health.RemoveHediff(toHeal);
            }
            else
            {
                // Try to restore a missing part
                var missingParts = pawn.health.hediffSet.GetMissingPartsCommonAncestors()
                    .Select(m => m.Part).ToList();
                if (missingParts.Any())
                {
                    pawn.health.RestorePart(missingParts.RandomElement());
                }
            }

            FleckMaker.AttachedOverlay(pawn, FleckDefOf.PsycastAreaEffect, UnityEngine.Vector3.zero);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return base.Valid(target, throwMessages) && target.Pawn != null;
        }
    }
}
