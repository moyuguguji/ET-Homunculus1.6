using RimWorld;
using Verse;

namespace ET_Homunculus
{
    public class CompUseEffect_AddHediff : CompUseEffect
    {
        private CompProperties_UseEffectAddHediff Props => (CompProperties_UseEffectAddHediff)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (Props.hediffDef == null || usedBy?.health == null) return;

            // Remove replaced (lower-tier) hediffs before installing
            if (Props.replacesHediffs != null)
            {
                foreach (var hDef in Props.replacesHediffs)
                {
                    var h = usedBy.health.hediffSet.GetFirstHediffOfDef(hDef);
                    if (h != null) usedBy.health.RemoveHediff(h);
                }
            }

            usedBy.health.AddHediff(Props.hediffDef, null, null);
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (Props.hediffDef == null)
                return new AcceptanceReport("ET_NoHediffDefined".Translate());

            if (p?.health == null)
                return new AcceptanceReport("ET_NoHealthTracker".Translate());

            if (p.health.hediffSet.HasHediff(Props.hediffDef))
                return new AcceptanceReport("ET_AlreadyInstalled".Translate());

            if (Props.allowedRaces != null && Props.allowedRaces.Count > 0)
            {
                if (p.def == null || !Props.allowedRaces.Contains(p.def.defName))
                    return new AcceptanceReport("ET_IncompatibleRace".Translate());
            }

            if (Props.blockedByHediffs != null)
            {
                foreach (var hDef in Props.blockedByHediffs)
                {
                    if (p.health.hediffSet.HasHediff(hDef))
                        return new AcceptanceReport("ET_HigherTierInstalled".Translate());
                }
            }

            return base.CanBeUsedBy(p);
        }
    }
}
