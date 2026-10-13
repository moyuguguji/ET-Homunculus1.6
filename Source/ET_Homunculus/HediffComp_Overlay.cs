using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ET_Homunculus
{
    public class HediffCompProperties_Overlay : HediffCompProperties
    {
        public string texPath;
        public float drawSize = 1.2f;
        public HediffCompProperties_Overlay() { compClass = typeof(HediffComp_Overlay); }
    }

    public class HediffComp_Overlay : HediffComp
    {
        private static Dictionary<string, Graphic> graphicCache = new Dictionary<string, Graphic>();
        private Graphic graphic;

        public HediffCompProperties_Overlay OProps => (HediffCompProperties_Overlay)props;

        public Graphic GetGraphic()
        {
            if (OProps.texPath == null || Pawn == null) return null;
            var key = OProps.texPath + "_" + Pawn.def.defName;
            if (!graphicCache.TryGetValue(key, out graphic))
            {
                var gd = new GraphicData
                {
                    texPath = key,
                    graphicClass = typeof(Graphic_Multi),
                    drawSize = new Vector2(OProps.drawSize, OProps.drawSize),
                    shaderType = ShaderTypeDefOf.CutoutComplex
                };
                graphic = gd.Graphic;
                graphicCache[key] = graphic;
            }
            return graphic;
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            Pawn?.Drawer?.renderer?.SetAllGraphicsDirty();
        }

        public override void CompPostPostRemoved()
        {
            Pawn?.Drawer?.renderer?.SetAllGraphicsDirty();
            base.CompPostPostRemoved();
        }
    }
}

