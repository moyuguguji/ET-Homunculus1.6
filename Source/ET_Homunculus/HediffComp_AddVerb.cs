using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace ET_Homunculus
{
    public class HediffCompProperties_AddVerb : HediffCompProperties
    {
        public List<VerbProperties> verbProps;
        public HediffCompProperties_AddVerb() { compClass = typeof(HediffComp_AddVerb); }
    }

    public class HediffComp_AddVerb : HediffComp
    {
        private bool added;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (added || Pawn == null) return;

            var props = (HediffCompProperties_AddVerb)this.props;
            if (props.verbProps == null) return;

            var verber = Pawn.verbTracker;
            if (verber == null) return;

            foreach (var vp in props.verbProps)
            {
                var verb = (Verb)System.Activator.CreateInstance(vp.verbClass);
                verb.caster = Pawn;
                vp.hasStandardCommand = true;
                verb.verbProps = vp;
                verb.verbTracker = verber;

                var verbsField = typeof(VerbTracker).GetField("verbs",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var list = verbsField?.GetValue(verber) as List<Verb>;
                list?.Add(verb);
            }

            added = true;
        }
    }
}
