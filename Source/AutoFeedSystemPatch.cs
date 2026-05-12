using HarmonyLib;
using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Harmony патч для Auto-Feed System - перехватывает выстрелы и уменьшает заряды
    /// </summary>
    [HarmonyPatch(typeof(Verb_Shoot))]
    [HarmonyPatch("TryCastShot")]
    public static class AutoFeedSystemPatch
    {
        static AutoFeedSystemPatch()
        {
            Log.Message("[ASE] AutoFeedSystemPatch: Static constructor called");
        }

        [HarmonyPostfix]
        public static void Postfix(Verb_Shoot __instance, ref bool __result)
        {
            try
            {
                // Если выстрел не удался, не тратим заряд
                if (!__result)
                    return;

                // Получаем пешку-стрелка
                Pawn casterPawn = __instance.CasterPawn;
                if (casterPawn == null || casterPawn.genes == null)
                    return;

                // Проверяем, есть ли ген Auto-Feed System
                GeneDef autoFeedDef = DefDatabase<GeneDef>.GetNamedSilentFail("ASE_AutoFeedSystem");
                if (autoFeedDef == null)
                    return;

                if (!casterPawn.genes.HasActiveGene(autoFeedDef))
                    return;

                // Получаем hediff с зарядами
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_AutoFeedSystemHediff");
                if (hediffDef == null)
                    return;

                Hediff hediff = casterPawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff == null)
                    return;

                // Уменьшаем заряд
                if (hediff.Severity > 0f)
                {
                    hediff.Severity = System.Math.Max(0f, hediff.Severity - 1f);
                    
                    if (AndroidSubroutinesExpandedMod.Settings == null || 
                        AndroidSubroutinesExpandedMod.Settings.enableGeneralLogging)
                    {
                        Log.Message("[ASE] Auto-Feed System: " + casterPawn.LabelShort + 
                                   " consumed charge (remaining: " + (int)hediff.Severity + "/5)");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("[ASE] AutoFeedSystemPatch error: " + ex.ToString());
            }
        }
    }
}

