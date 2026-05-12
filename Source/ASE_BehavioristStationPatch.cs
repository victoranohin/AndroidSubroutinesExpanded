using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Патч для Building_AndroidBehavioristStation.FinishAndroidProject()
    /// Удаляет все ASE гены перед тем, как родительский мод удалит гены из Utils.allAndroidGenes
    /// Применяется вручную, чтобы избежать вызова статического конструктора Building_AndroidBehavioristStation
    /// </summary>
    public static class ASE_BehavioristStationPatch
    {
        /// <summary>
        /// Регистрирует патч вручную после загрузки всех ресурсов
        /// </summary>
        public static void RegisterPatch(Harmony harmony)
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                try
                {
                    // Используем AccessTools для получения типа без вызова статического конструктора
                    var targetType = AccessTools.TypeByName("VREAndroids.Building_AndroidBehavioristStation");
                    if (targetType == null)
                    {
                        Log.Error("[ASE] Building_AndroidBehavioristStation type not found!");
                        return;
                    }

                    var targetMethod = AccessTools.Method(targetType, "FinishAndroidProject");
                    if (targetMethod == null)
                    {
                        Log.Error("[ASE] FinishAndroidProject method not found!");
                        return;
                    }

                    var prefixMethod = AccessTools.Method(typeof(ASE_BehavioristStationPatch), "Prefix");
                    harmony.Patch(targetMethod, new HarmonyMethod(prefixMethod) { priority = Priority.First });
                    
                    Log.Message("[ASE] BehavioristStationPatch registered successfully");
                }
                catch (System.Exception ex)
                {
                    Log.Error("[ASE] Failed to register BehavioristStationPatch: " + ex.ToString());
                }
            });
        }

        /// <summary>
        /// Prefix: удаляем все ASE гены перед выполнением родительского кода
        /// </summary>
        public static void Prefix(Building_AndroidBehavioristStation __instance)
        {
            try
            {
                var android = __instance.Occupant;
                if (android == null || android.genes == null)
                {
                    return;
                }

                // Собираем все ASE гены, которые есть у андроида
                var aseGenesToRemove = new List<Gene>();
                foreach (var gene in android.genes.GenesListForReading)
                {
                    if (gene.def != null && gene.def.defName != null && gene.def.defName.StartsWith("ASE_"))
                    {
                        aseGenesToRemove.Add(gene);
                    }
                }

                // Удаляем все ASE гены
                int removedCount = 0;
                foreach (var gene in aseGenesToRemove)
                {
                    android.genes.RemoveGene(gene);
                    removedCount++;
                    
                    if (AndroidSubroutinesExpandedMod.Settings != null && 
                        AndroidSubroutinesExpandedMod.Settings.enableGeneralLogging)
                    {
                        Log.Message("[ASE] Removed gene " + gene.def.defName + " from " + android.LabelShort);
                    }
                }

                if (removedCount > 0)
                {
                    Log.Message("[ASE] Removed " + removedCount + " ASE genes from " + android.LabelShort + " during modification");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("[ASE] BehavioristStationPatch error: " + ex.ToString());
            }
        }
    }
}
