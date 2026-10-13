using RimWorld;
using Verse;

namespace ET_Homunculus
{
    public class CompAbilityEffect_Jump : CompAbilityEffect
    {
        public new CompProperties_AbilityJump Props => (CompProperties_AbilityJump)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var pawn = parent.pawn;
            if (pawn == null || !target.IsValid) return;
            var cell = target.Cell;
            if (!cell.IsValid || !cell.Standable(pawn.Map)) return;
            FleckMaker.Static(cell, pawn.Map, FleckDefOf.ExplosionFlash);
            pawn.Position = cell;
            pawn.stances.stunner.StunFor(20, pawn, false, false);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages)) return false;
            var pawn = parent.pawn;
            if (pawn == null) return false;
            if (pawn.Position.DistanceTo(target.Cell) > Props.range) return false;
            return true;
        }
    }

    public class CompProperties_AbilityJump : CompProperties_AbilityEffect
    {
        public float range = 28f;
        public CompProperties_AbilityJump() { compClass = typeof(CompAbilityEffect_Jump); }
    }
}
