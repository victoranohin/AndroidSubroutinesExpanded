using HarmonyLib;
using RimWorld;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    [HarmonyPatch(typeof(JobDriver_RepairAndroid), "RepairTick")]
    public static class AntiInsectoidPlatesRepairPatch
    {
        // Все типы пластин
        private static readonly string[] PlatesHediffNames = new string[]
        {
            "ASE_AntiInsectoidPlates_MkI",
            "ASE_AntiInsectoidPlates_MkII",
            "ASE_AntiInsectoidPlates_MkIII"
        };
        
        private static bool ShouldLog()
        {
            return AndroidSubroutinesExpandedMod.Settings != null && 
                   AndroidSubroutinesExpandedMod.Settings.enableAntiInsectoidPlatesLogging;
        }
        
        private static bool ShouldShowMessages()
        {
            return AndroidSubroutinesExpandedMod.Settings != null && 
                   AndroidSubroutinesExpandedMod.Settings.enableAntiInsectoidPlatesMessages;
        }
        
        /// <summary>
        /// Находит любой hediff Anti-Insectoid Plates
        /// </summary>
        private static Hediff_AntiInsectoidPlates FindAnyPlatesHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null) return null;
            
            foreach (string hediffName in PlatesHediffNames)
            {
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffName);
                if (hediffDef != null)
                {
                    Hediff_AntiInsectoidPlates plates = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) as Hediff_AntiInsectoidPlates;
                    if (plates != null)
                    {
                        return plates;
                    }
                }
            }
            
            return null;
        }
        
        public static void Postfix(Pawn android)
        {
            try
            {
                if (android == null || android.health == null) return;
                
                // Ищем любой Anti-Insectoid Plates hediff
                Hediff_AntiInsectoidPlates plates = FindAnyPlatesHediff(android);
                
                if (plates == null)
                {
                    // Нет пластин - ничего не делаем
                    return;
                }
                
                float maxArmor = plates.GetMaxArmorForGenes();
                float currentArmor = plates.Severity;
                
                if (ShouldLog())
                {
                    Log.Message("[ASE PLATES REPAIR] RepairTick called for " + android.LabelShort + 
                               " | Current: " + currentArmor.ToString("F1") + 
                               " | Max: " + maxArmor.ToString("F1") +
                               " | NeedsRepair: " + (currentArmor < maxArmor));
                }
                
                // Ремонтируем если есть гены и броня меньше максимума
                if (maxArmor > 0f && currentArmor < maxArmor)
                {
                    // Находим андроида, который ремонтирует
                    Pawn repairer = null;
                    if (android.Map != null)
                    {
                        var pawnsNearby = android.Map.mapPawns.AllPawnsSpawned;
                        foreach (var pawn in pawnsNearby)
                        {
                            if (pawn != android && pawn.CurJob != null && 
                                pawn.CurJob.def.defName == "RepairAndroid" &&
                                pawn.CurJob.targetA.Thing == android)
                            {
                                repairer = pawn;
                                break;
                            }
                        }
                    }
                    
                    // Если ремонтирует сам себя, используем свой навык
                    if (repairer == null)
                        repairer = android;
                    
                    // Скорость ремонта = навык ремесло
                    int craftingLevel = repairer.skills != null ? repairer.skills.GetSkill(SkillDefOf.Crafting).Level : 0;
                    float repairAmount = craftingLevel > 0 ? craftingLevel : 0.5f;
                    
                    float oldSeverity = plates.Severity;
                    plates.RepairArmor(repairAmount);
                    float newSeverity = plates.Severity;
                    
                    if (ShouldLog())
                    {
                        Log.Message("[ASE PLATES REPAIR] " + android.LabelShort + 
                                   " | Repairer: " + repairer.LabelShort + 
                                   " (Crafting " + craftingLevel + ")" +
                                   " | RepairAmount: " + repairAmount.ToString("F1") +
                                   " | Before: " + oldSeverity.ToString("F1") + 
                                   " | After: " + newSeverity.ToString("F1") +
                                   " | Max: " + maxArmor.ToString("F1"));
                    }
                    
                    if (ShouldShowMessages() && android.Faction == Faction.OfPlayer)
                    {
                        Messages.Message(android.LabelShort + " plates +" + 
                                       repairAmount.ToString("F0") + " (" + 
                                       newSeverity.ToString("F0") + "/" + maxArmor.ToString("F0") + ")", 
                                       android, MessageTypeDefOf.PositiveEvent);
                    }
                }
                else if (ShouldLog() && maxArmor > 0f)
                {
                    Log.Message("[ASE PLATES REPAIR] " + android.LabelShort + 
                               " | Skipped - armor already at max (" + currentArmor.ToString("F1") + "/" + maxArmor.ToString("F1") + ")");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("[ASE PLATES REPAIR] Error: " + ex.ToString());
            }
        }
    }
}
