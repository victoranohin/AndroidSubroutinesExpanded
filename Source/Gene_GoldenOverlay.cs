using Verse;
using RimWorld;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Gene_GoldenOverlay : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                // Создаем hediff для Golden Overlay
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_GoldenOverlayHediff"), 
                    pawn);
                
                if (hediff != null)
                {
                    hediff.Severity = 100f;
                    pawn.health.AddHediff(hediff);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                // Удаляем hediff
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_GoldenOverlayHediff"));
                
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }
}

