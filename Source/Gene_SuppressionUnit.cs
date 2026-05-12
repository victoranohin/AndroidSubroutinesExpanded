using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_SuppressionUnit : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_SuppressionUnitHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 1f;
                    pawn.health.AddHediff(hediff);
                    Log.Message("[ASE] Suppression Unit activated on " + pawn.LabelShort);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_SuppressionUnitHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    Log.Message("[ASE] Suppression Unit removed from " + pawn.LabelShort);
                }
            }
        }
    }
}

