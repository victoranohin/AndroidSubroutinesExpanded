using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_Overdrive_MkI : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                // Создание hediff
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_OverdriveMkIHediff"), 
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 100f;
                    pawn.health.AddHediff(hediff);
                    Log.Message("[ASE] Overdrive MkI: Created hediff for " + pawn.LabelShort);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                // Удаление hediff
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_OverdriveMkIHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    Log.Message("[ASE] Overdrive MkI: Removed hediff from " + pawn.LabelShort);
                }
            }
        }
    }
}

