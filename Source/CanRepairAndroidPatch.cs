using HarmonyLib;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Патч для JobDriver_RepairAndroid.CanRepairAndroid
    /// Добавляет проверку повреждённых Anti-Insectoid Plates
    /// Теперь врач будет чинить андроида если пластины повреждены, даже если здоровье полное
    /// </summary>
    [HarmonyPatch(typeof(JobDriver_RepairAndroid), "CanRepairAndroid")]
    public static class CanRepairAndroidPatch
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
        
        /// <summary>
        /// Postfix - если оригинальный метод вернул false, проверяем пластины
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(Pawn android, ref bool __result)
        {
            // Если уже нужен ремонт - не меняем результат
            if (__result) return;
            
            // Проверяем наличие повреждённых пластин
            Hediff_AntiInsectoidPlates plates = FindAnyPlatesHediff(android);
            
            if (plates != null && plates.NeedsRepair())
            {
                __result = true;
                
                if (ShouldLog())
                {
                    float percent = plates.GetArmorPercent();
                    Log.Message("[ASE PLATES] CanRepairAndroid override: " + android.LabelShort + 
                               " needs repair (" + plates.def.label + " at " + percent.ToString("F0") + "%)");
                }
            }
        }
    }
}

