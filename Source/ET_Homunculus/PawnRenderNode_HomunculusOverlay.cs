using RimWorld;
using Verse;

namespace ET_Homunculus
{
    /// <summary>
    /// Draws one implant armour overlay texture on top of a pawn's body.
    ///
    /// This is the replacement for the old (dead) pair
    /// <c>DynamicPawnRenderNodeSetup_HomunculusOverlay</c> + <c>HediffComp_Overlay</c>.
    /// RimWorld 1.7 discovers these nodes through the owning HediffDef's own
    /// <c>&lt;renderNodeProperties&gt;</c> block, so a node is created automatically for
    /// every pawn carrying the implant -- no PawnRenderTreeDef patch is needed, and the
    /// hediff comp is gone entirely.
    ///
    /// Context supplied by the framework:
    ///   <c>tree.pawn</c>       - the pawn being drawn
    ///   <c>hediff</c>          - the HediffDef whose renderNodeProperties created this
    ///                            node, which is how one class serves all armour tiers
    ///   <c>props.texPath</c>   - that tier's base texture, from the HediffDef XML
    ///   <c>props.drawSize</c>  - that tier's draw size, from the HediffDef XML
    ///
    /// The texture is resolved as "&lt;texPath&gt;_&lt;pawn.def.defName&gt;", giving one
    /// skin per homunculus race (…_Nymph / …_Oread / …_Naiad) out of a single node.
    /// </summary>
    public class PawnRenderNode_HomunculusOverlay : PawnRenderNode
    {
        private Graphic graphicInt;
        private bool graphicResolved;

        public PawnRenderNode_HomunculusOverlay(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (!graphicResolved)
            {
                graphicResolved = true;
                graphicInt = ResolveGraphic(pawn);
            }
            return graphicInt;
        }

        private Graphic ResolveGraphic(Pawn pawn)
        {
            if (pawn?.def == null || props == null || props.texPath.NullOrEmpty())
            {
                return null;
            }

            // Only the homunculus races have art for these overlays, and only when the
            // layer passes the player's show/hide settings.
            if (!HomunculusRaceDefs.IsHomunculus(pawn))
            {
                return null;
            }
            OverlayLayer layer = HomunculusOverlayLayers.ForHediff(hediff?.def);
            if (layer == null || !HomunculusMod.IsLayerVisible(layer, pawn.def.defName))
            {
                return null;
            }

            var data = new GraphicData
            {
                texPath = props.texPath + "_" + pawn.def.defName,
                graphicClass = typeof(Graphic_Multi),
                shaderType = ShaderTypeDefOf.CutoutComplex,
                drawSize = props.drawSize.x > 0f ? props.drawSize : new UnityEngine.Vector2(1.2f, 1.2f),
            };

            Graphic graphic = data.Graphic;
            // A missing texture degrades to BadGraphic; that means "no art for this
            // race/tier", which is a normal case (e.g. no Oread Armor II skin).
            if (graphic == null || graphic == BaseContent.BadGraphic)
            {
                return null;
            }
            return graphic;
        }

        /// <summary>Drop the cached graphic so the next draw re-reads the settings.</summary>
        public void InvalidateGraphic()
        {
            graphicResolved = false;
            graphicInt = null;
        }
    }
}
