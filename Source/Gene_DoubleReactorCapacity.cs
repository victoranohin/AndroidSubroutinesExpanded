using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_DoubleReactorCapacity : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DoubleReactorCapacityHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 100f;
                    pawn.health.AddHediff(hediff);
                    Log.Message("[ASE] Double Reactor Capacity: hediff added to " + pawn.LabelShort);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DoubleReactorCapacityHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    Log.Message("[ASE] Double Reactor Capacity: hediff removed from " + pawn.LabelShort);
                }
            }
        }
    }
}
