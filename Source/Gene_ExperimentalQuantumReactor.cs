using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Experimental Quantum Reactor: +12 power efficiency, reformatting time ×3
    /// Reformatting time handled by QuantumReactorReformattingPatch
    /// </summary>
    public class Gene_ExperimentalQuantumReactor : Gene
    {
        private bool ShouldLogGeneral()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableGeneralLogging;
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ExperimentalQuantumReactorHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 100f;
                    pawn.health.AddHediff(hediff);
                    if (ShouldLogGeneral())
                    {
                        Log.Message("[ASE] Experimental Quantum Reactor: hediff added to " + pawn.LabelShort);
                    }
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ExperimentalQuantumReactorHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    if (ShouldLogGeneral())
                    {
                        Log.Message("[ASE] Experimental Quantum Reactor: hediff removed from " + pawn.LabelShort);
                    }
                }
            }
        }
    }
}

