using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_BarrageProtocol : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_BarrageProtocolHediff");
                if (hediffDef != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                    hediff.Severity = 1f;
                    pawn.health.AddHediff(hediff);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_BarrageProtocolHediff");
                if (hediffDef != null)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
}

