using Verse;
using RimWorld;
using System.Collections.Generic;
using System;

namespace AndroidSubroutinesExpanded
{
    public class Gene_AutoRepairModule : Gene
    {
        private int tickCounter = 0;
        private int limbTickCounter = 0;
        private const int REPAIR_TICK_INTERVAL = 100;
        private const int LIMB_REPAIR_TICK_INTERVAL = 5000;

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                // Создаем hediff для Auto-Repair Module
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_AutoRepairModuleHediff"), 
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
                // Удаляем hediff
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_AutoRepairModuleHediff"));
                
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            if (pawn == null || pawn.health == null || pawn.Dead)
                return;

            tickCounter++;
            limbTickCounter++;

            // Обычное восстановление каждые 100 тиков
            if (tickCounter >= REPAIR_TICK_INTERVAL)
            {
                tickCounter = 0;
                RepairDamage();
            }

            // Восстановление конечностей каждые 5000 тиков
            if (limbTickCounter >= LIMB_REPAIR_TICK_INTERVAL)
            {
                limbTickCounter = 0;
                RepairMissingBodyParts();
            }
        }

        private void RepairDamage()
        {
            if (pawn.health.hediffSet.hediffs.Count == 0)
                return;

            // Ищем повреждения для восстановления
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                Hediff_Injury injury = hediff as Hediff_Injury;
                if (injury != null && injury.Severity > 0)
                {
                    injury.Severity = Math.Max(0, injury.Severity - 1f);
                    break; // Восстанавливаем только одно повреждение за раз
                }
            }
        }

        private void RepairMissingBodyParts()
        {
            List<Hediff_MissingPart> missingParts = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
            if (missingParts == null || missingParts.Count == 0)
                return;

            // Восстанавливаем одну отсутствующую часть тела - просто удаляем MissingPart hediff
            Hediff_MissingPart partToRestore = missingParts[0];
            if (partToRestore != null)
            {
                pawn.health.RemoveHediff(partToRestore);
            }
        }
    }
}
