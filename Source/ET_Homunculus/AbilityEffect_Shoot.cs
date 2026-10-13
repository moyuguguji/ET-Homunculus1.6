using RimWorld;
using Verse;
using UnityEngine;

namespace ET_Homunculus
{
    public class CompAbilityEffect_LaunchProjectile : CompAbilityEffect
    {
        public new CompProperties_LaunchProjectile Props => (CompProperties_LaunchProjectile)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var pawn = parent.pawn;
            if (pawn == null) return;

            var projectile = ThingMaker.MakeThing(Props.projectileDef);
            var dir = (target.Cell.ToVector3Shifted() - pawn.DrawPos).normalized;
            var origin = pawn.DrawPos + dir;
            GenSpawn.Spawn(projectile, pawn.Position, pawn.Map);

            if (projectile is Projectile proj)
            {
                var targ = target.Thing != null ? new LocalTargetInfo(target.Thing) : target;
                proj.Launch(pawn, origin, targ, targ, ProjectileHitFlags.All);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return base.Valid(target, throwMessages) && target.IsValid;
        }
    }

    public class CompProperties_LaunchProjectile : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        public CompProperties_LaunchProjectile() { compClass = typeof(CompAbilityEffect_LaunchProjectile); }
    }
}
