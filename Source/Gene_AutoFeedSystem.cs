using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_AutoFeedSystem : Gene
    {
        private const int RechargeInterval = 200; // ticks between charges
        private int tickCounter = 0;

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                // Проверяем, не существует ли уже hediff
                Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_AutoFeedSystemHediff"));
                
                if (existingHediff == null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(
                        DefDatabase<HediffDef>.GetNamedSilentFail("ASE_AutoFeedSystemHediff"),
                        pawn);
                    if (hediff != null)
                    {
                        hediff.Severity = 5f; // Start with full charges (5)
                        pawn.health.AddHediff(hediff);
                    }
                }
                else
                {
                    // Если hediff уже существует, просто обновляем severity до максимума
                    existingHediff.Severity = 5f;
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_AutoFeedSystemHediff"));
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
                DefDatabase<HediffDef>.GetNamedSilentFail("ASE_AutoFeedSystemHediff"));
            
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

        public void ConsumeCharge()
        {
            if (pawn == null || pawn.health == null)
            {
                return;
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamedSilentFail("ASE_AutoFeedSystemHediff"));
            
            if (hediff != null && hediff.Severity > 0f)
            {
                hediff.Severity = System.Math.Max(0f, hediff.Severity - 1f);
            }
        }

        public bool HasCharges()
        {
            if (pawn == null || pawn.health == null)
            {
                return false;
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamedSilentFail("ASE_AutoFeedSystemHediff"));
            
            return hediff != null && hediff.Severity > 0f;
        }
    }
}

