using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_HeavyPlating_MkI : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_HeavyPlatingMkIHediff"),
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
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_HeavyPlatingMkIHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }
}

