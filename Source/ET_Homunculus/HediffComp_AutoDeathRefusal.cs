using RimWorld;
using Verse;

namespace ET_Homunculus
{
    public class HediffCompProperties_Deathless : HediffCompProperties
    {
        public HediffCompProperties_Deathless() { compClass = typeof(HediffComp_Deathless); }
    }

    public class HediffComp_Deathless : HediffComp
    {
        public override void Notify_PawnKilled()
        {
            if (Pawn?.Dead != true) return;
            var brain = Pawn.health?.hediffSet?.GetBrain();
            if (brain == null) return;

            ResurrectionUtility.TryResurrect(Pawn);
            var comaDef = DefDatabase<HediffDef>.GetNamedSilentFail("RegenerationComa");
            if (comaDef != null)
                Pawn.health.AddHediff(comaDef);
        }
    }
}
