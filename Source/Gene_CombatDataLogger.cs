using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_CombatDataLogger : Gene
    {
        public int killCount = 0;
        private HediffDef hediffDef;

        public override void PostAdd()
        {
            base.PostAdd();
            InitializeHediff();
            UpdateHediff();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            RemoveHediff();
        }

        private void InitializeHediff()
        {
            if (hediffDef == null)
            {
                hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_CombatDataLoggerHediff");
            }
        }

        public void OnEnemyKilled()
        {
            killCount++;
            UpdateHediff();
        }

        private void UpdateHediff()
        {
            if (pawn == null || pawn.health == null)
                return;

            InitializeHediff();
            if (hediffDef == null)
                return;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                hediff.Severity = 0.1f;
                pawn.health.AddHediff(hediff);
            }
            
            if (killCount > 0)
            {
                hediff.Severity = killCount;
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref killCount, "killCount", 0);
        }
    }
}
