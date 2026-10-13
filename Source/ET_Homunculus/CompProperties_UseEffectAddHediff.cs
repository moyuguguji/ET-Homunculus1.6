using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ET_Homunculus
{
    public class CompProperties_UseEffectAddHediff : CompProperties_UseEffect
    {
        public HediffDef hediffDef;
        public List<string> allowedRaces;
        public List<HediffDef> replacesHediffs;
        public List<HediffDef> blockedByHediffs;

        public CompProperties_UseEffectAddHediff()
        {
            compClass = typeof(CompUseEffect_AddHediff);
        }
    }
}
