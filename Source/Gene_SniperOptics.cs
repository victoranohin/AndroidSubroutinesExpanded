using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_SniperOptics : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_SniperOpticsHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 1f;
                    pawn.health.AddHediff(hediff);
                    Log.Message("[ASE] Sniper Optics activated on " + pawn.LabelShort + " -> Shooting accuracy +20%, Incoming damage +50%, Move speed -30%");
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_SniperOpticsHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }
}

