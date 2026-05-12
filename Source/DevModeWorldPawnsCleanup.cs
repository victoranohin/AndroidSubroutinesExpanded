using RimWorld;
using System.Collections.Generic;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Dev mode команда для очистки WorldPawns от андроидов с избыточными hediffs
    /// 
    /// ИСПОЛЬЗОВАНИЕ:
    /// 1. Включите Dev Mode (Settings -> Enable Dev Mode)
    /// 2. Нажмите / (слэш) чтобы открыть Debug Actions меню
    /// 3. Найдите команду "ASE: Cleanup WorldPawns" в категории "Tools"
    /// 
    /// ИЛИ используйте Developer Console (если установлен мод):
    /// AndroidSubroutinesExpanded.DevModeWorldPawnsCleanup.CleanupWorldPawns()
    /// </summary>
    public class DevModeWorldPawnsCleanup
    {
        private const int MAX_HEDIFFS_THRESHOLD = 500;

        /// <summary>
        /// Статический метод для вызова из dev mode команды или Developer Console
        /// </summary>
        public static void CleanupWorldPawns()
        {
            if (Find.WorldPawns == null)
            {
                Log.Warning("[ASE DevMode] WorldPawns is null!");
                return;
            }

            List<Pawn> androidsToRemove = new List<Pawn>();
            int totalChecked = 0;
            int totalAndroids = 0;

            // Проверяем всех андроидов в WorldPawns
            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAliveOrDead)
            {
                totalChecked++;
                
                if (pawn == null || !pawn.IsAndroid())
                    continue;

                totalAndroids++;

                if (pawn.health == null || pawn.health.hediffSet == null || pawn.health.hediffSet.hediffs == null)
                    continue;

                int hediffCount = pawn.health.hediffSet.hediffs.Count;
                
                if (hediffCount > MAX_HEDIFFS_THRESHOLD)
                {
                    Log.Warning("[ASE DevMode] Found android " + pawn.LabelShort + " in WorldPawns with " + hediffCount + " hediffs. Marking for removal...");
                    androidsToRemove.Add(pawn);
                }
            }

            // Удаляем проблемных андроидов
            int removedCount = 0;
            foreach (Pawn pawn in androidsToRemove)
            {
                try
                {
                    Log.Message("[ASE DevMode] Removing " + pawn.LabelShort + " from WorldPawns due to excessive hediffs (" + pawn.health.hediffSet.hediffs.Count + " hediffs).");
                    Find.WorldPawns.RemovePawn(pawn);
                    Find.WorldPawns.PassToWorld(pawn);
                    removedCount++;
                }
                catch (System.Exception ex)
                {
                    string pawnLabel = (pawn != null) ? pawn.LabelShort : "null";
                    Log.Error("[ASE DevMode] Error removing pawn " + pawnLabel + ": " + ex.ToString());
                }
            }

            // Выводим итоговую информацию
            Messages.Message(
                "[ASE DevMode] Cleanup complete. Checked: " + totalChecked + " pawns, Found: " + totalAndroids + " androids, Removed: " + removedCount + " problematic androids",
                MessageTypeDefOf.NeutralEvent);
            
            Log.Message("[ASE DevMode] WorldPawns cleanup complete. Checked " + totalChecked + " pawns, found " + totalAndroids + " androids, removed " + removedCount + " problematic androids.");
        }
    }

}
