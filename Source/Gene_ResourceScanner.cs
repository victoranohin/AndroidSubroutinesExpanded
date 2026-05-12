using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Gene_ResourceScanner : Gene
    {
        private const int SCAN_INTERVAL = 60000; // 1 день
        private int ticksUntilNextScan = SCAN_INTERVAL;

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ResourceScannerHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 100f;
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
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ResourceScannerHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            if (pawn == null || pawn.Dead || pawn.Map == null)
                return;

            ticksUntilNextScan--;
            if (ticksUntilNextScan <= 0)
            {
                ticksUntilNextScan = SCAN_INTERVAL;
                TryScanForResources();
            }
        }

        private void TryScanForResources()
        {
            if (pawn == null || pawn.Map == null)
                return;

            Map map = pawn.Map;
            
            if (map.deepResourceGrid == null)
                return;

            List<ThingDef> allDeepResources = new List<ThingDef>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.deepCommonality > 0f && def.deepCountPerPortion > 0)
                {
                    allDeepResources.Add(def);
                }
            }

            if (allDeepResources.Count == 0)
            {
                Messages.Message("[ASE] " + pawn.LabelShort + "'s resource scanner found no available resources.", MessageTypeDefOf.NeutralEvent);
                return;
            }

            ThingDef chosenResource = allDeepResources.RandomElementByWeight((ThingDef d) => d.deepCommonality);

            List<IntVec3> availableCells = new List<IntVec3>();
            foreach (IntVec3 cell in map.AllCells)
            {
                if (map.deepResourceGrid.ThingDefAt(cell) == null && cell.InBounds(map))
                {
                    availableCells.Add(cell);
                }
            }

            if (availableCells.Count == 0)
            {
                Messages.Message("[ASE] " + pawn.LabelShort + "'s resource scanner found no suitable locations.", MessageTypeDefOf.NeutralEvent);
                return;
            }

            IntVec3 chosenCell = availableCells.RandomElement();
            
            map.deepResourceGrid.SetAt(chosenCell, chosenResource, chosenResource.deepCountPerPortion);

            string resourceName = chosenResource.label;
            if (string.IsNullOrEmpty(resourceName))
            {
                resourceName = chosenResource.defName;
            }
            
            Messages.Message("[ASE] " + pawn.LabelShort + "'s scanner detected " + resourceName + " deposit at (" + chosenCell.x + ", " + chosenCell.z + ")!", 
                new TargetInfo(chosenCell, map, false), MessageTypeDefOf.PositiveEvent);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksUntilNextScan, "ticksUntilNextScan", SCAN_INTERVAL);
        }
    }
}
