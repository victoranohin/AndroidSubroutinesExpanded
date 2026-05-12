using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_PropagandaBroadcaster : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_PropagandaBroadcasterHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 1f;
                    pawn.health.AddHediff(hediff);
                    Log.Message("[ASE] Propaganda Broadcaster activated on " + pawn.LabelShort);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_PropagandaBroadcasterHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    Log.Message("[ASE] Propaganda Broadcaster removed from " + pawn.LabelShort);
                }
            }
        }
    }
}

