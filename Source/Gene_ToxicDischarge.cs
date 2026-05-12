using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_ToxicDischarge : Gene
    {
        private const int RechargeInterval = 1000; // ticks between charges (5x slower than Auto Feed System)
        private int tickCounter = 0;

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ToxicDischargeHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 5f; // Start with full charges (5)
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
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ToxicDischargeHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn == null || pawn.health == null)
            {
                return;
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ToxicDischargeHediff"));
            
            if (hediff != null && hediff.Severity < 5f)
            {
                tickCounter++;
                if (tickCounter >= RechargeInterval)
                {
                    hediff.Severity = System.Math.Min(5f, hediff.Severity + 1f);
                    tickCounter = 0;
                }
            }
            else
            {
                tickCounter = 0;
            }
        }

        /// <summary>
        /// Потребляет заряд и создаёт токсичное облако
        /// </summary>
        public bool TryConsumeCharge()
        {
            if (pawn == null || pawn.health == null)
            {
                return false;
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ToxicDischargeHediff"));
            
            if (hediff != null && hediff.Severity > 0f)
            {
                hediff.Severity = System.Math.Max(0f, hediff.Severity - 1f);
                return true;
            }
            
            return false;
        }

        public bool HasCharges()
        {
            if (pawn == null || pawn.health == null)
            {
                return false;
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ToxicDischargeHediff"));
            
            return hediff != null && hediff.Severity > 0f;
        }
    }
}

