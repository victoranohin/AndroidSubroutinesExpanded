using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_BerserkerProtocol_T2 : Gene
    {
        private HediffDef hediffDef;
        private const float RAGE_CHANCE_PER_CHECK = 0.001f; // 0.1% per check when damaged
        private float lastHealthPercent = 1f;
        private bool initialized = false;

        public override void PostAdd()
        {
            base.PostAdd();
            InitializeHediff();
            AddHediff();
            if (pawn != null && pawn.health != null)
            {
                lastHealthPercent = pawn.health.summaryHealth.SummaryHealthPercent;
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            RemoveHediff();
        }

        public override void Tick()
        {
            base.Tick();
            
            if (pawn == null || pawn.Dead)
                return;

            if (pawn.IsHashIntervalTick(60))
            {
                UpdateAttackSpeedBonus();
                CheckForRageTrigger();
            }
        }

        private void InitializeHediff()
        {
            if (!initialized)
            {
                hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_BerserkerProtocol_T2_Hediff");
                initialized = true;
            }
        }

        private void AddHediff()
        {
            if (pawn == null || pawn.health == null)
                return;

            InitializeHediff();
            if (hediffDef == null)
                return;

            if (!pawn.health.hediffSet.HasHediff(hediffDef))
            {
                Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                hediff.Severity = 0.1f;
                pawn.health.AddHediff(hediff);
            }
        }

        private void RemoveHediff()
        {
            if (pawn == null || pawn.health == null)
                return;

            InitializeHediff();
            if (hediffDef == null)
                return;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }

        private void CheckForRageTrigger()
        {
            if (pawn == null || pawn.Dead || pawn.health == null || pawn.InMentalState)
                return;

            float currentHealth = pawn.health.summaryHealth.SummaryHealthPercent;
            
            // Trigger rage check only when taking damage (health decreased)
            if (currentHealth < lastHealthPercent)
            {
                if (Rand.Value < RAGE_CHANCE_PER_CHECK)
                {
                    TriggerBerserkRage();
                }
            }
            
            lastHealthPercent = currentHealth;
        }

        private void TriggerBerserkRage()
        {
            if (pawn == null || pawn.Dead || pawn.InMentalState)
                return;

            MentalStateDef berserkDef = DefDatabase<MentalStateDef>.GetNamedSilentFail("Berserk");
            if (berserkDef != null)
            {
                pawn.mindState.mentalStateHandler.TryStartMentalState(berserkDef, "ASE Berserker Protocol T2 triggered", true, false, false, null, false, false);
                
                Messages.Message("[ASE] " + pawn.LabelShort + " entered BERSERKER RAGE! They will attack anyone nearby!", 
                    new LookTargets(pawn), MessageTypeDefOf.ThreatBig);
            }
        }

        private void UpdateAttackSpeedBonus()
        {
            if (pawn == null || pawn.health == null)
                return;

            float healthPercent = pawn.health.summaryHealth.SummaryHealthPercent;
            float missingHealthPercent = (1f - healthPercent) * 100f;
            
            InitializeHediff();
            if (hediffDef == null)
                return;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            
            if (hediff == null)
            {
                AddHediff();
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            }
            
            if (hediff != null)
            {
                // Severity = missing health % (used by hediff to calculate attack speed bonus)
                hediff.Severity = missingHealthPercent > 0.1f ? missingHealthPercent : 0.1f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastHealthPercent, "lastHealthPercent", 1f);
        }
    }
}
