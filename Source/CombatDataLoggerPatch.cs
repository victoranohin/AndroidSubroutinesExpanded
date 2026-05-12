using HarmonyLib;
using Verse;
using RimWorld;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Harmony патч для отслеживания убийств и обновления Combat Data Logger
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class CombatDataLoggerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, DamageInfo? dinfo)
        {
            if (dinfo == null || !dinfo.HasValue)
                return;

            Pawn killer = dinfo.Value.Instigator as Pawn;
            if (killer == null || killer.genes == null)
                return;

            // Проверяем, что это враг
            if (__instance.Faction != null && killer.Faction != null && 
                !__instance.Faction.HostileTo(killer.Faction))
                return;

            // Ищем ген Combat Data Logger у убийцы
            foreach (Gene gene in killer.genes.GenesListForReading)
            {
                Gene_CombatDataLogger logger = gene as Gene_CombatDataLogger;
                if (logger != null)
                {
                    logger.OnEnemyKilled();
                    break;
                }
            }
        }
    }
}
