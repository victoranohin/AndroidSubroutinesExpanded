using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Gene_DecentralizedMind : Gene
    {
        private HediffDef hediffDef;

        public override void PostAdd()
        {
            base.PostAdd();
            InitializeHediff();
            AddHediff();
            UpdateAllAndroidsWithGene();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            RemoveHediff();
            UpdateAllAndroidsWithGene();
        }

        public override void Tick()
        {
            base.Tick();
            
            if (pawn == null || pawn.Dead || pawn.Map == null)
                return;

            if (pawn.IsHashIntervalTick(250))
            {
                UpdateWorkSpeedBonus();
            }
        }

        private void InitializeHediff()
        {
            if (hediffDef == null)
            {
                hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DecentralizedMindHediff");
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
                hediff.Severity = 1f;
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

        public void UpdateWorkSpeedBonus()
        {
            if (pawn == null || pawn.Map == null)
                return;

            int count = CountAndroidsWithGene();
            
            InitializeHediff();
            if (hediffDef == null)
                return;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            
            if (count > 0)
            {
                if (hediff == null)
                {
                    hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                    pawn.health.AddHediff(hediff);
                }
                hediff.Severity = count;
            }
            else if (hediff != null)
            {
                hediff.Severity = 1f;
            }
        }

        private int CountAndroidsWithGene()
        {
            if (pawn == null || pawn.Map == null)
                return 0;

            int count = 0;
            IEnumerable<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned;
            
            foreach (Pawn p in pawns)
            {
                if (p.Dead || p.genes == null)
                    continue;

                foreach (Gene gene in p.genes.GenesListForReading)
                {
                    if (gene.def.defName == "ASE_DecentralizedMind")
                    {
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        private void UpdateAllAndroidsWithGene()
        {
            if (pawn == null || pawn.Map == null)
                return;

            IEnumerable<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned;
            
            foreach (Pawn p in pawns)
            {
                if (p.Dead || p.genes == null)
                    continue;

                foreach (Gene gene in p.genes.GenesListForReading)
                {
                    Gene_DecentralizedMind decentralized = gene as Gene_DecentralizedMind;
                    if (decentralized != null)
                    {
                        decentralized.UpdateWorkSpeedBonus();
                        break;
                    }
                }
            }
        }
    }
}
