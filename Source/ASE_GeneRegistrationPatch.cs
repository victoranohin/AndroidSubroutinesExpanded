using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Патч для регистрации наших AndroidGeneDef в списке генов VREA.
    /// VREA не добавляет гены автоматически - нужно добавлять вручную.
    /// </summary>
    [HarmonyPatch(typeof(VREAndroids.Utils), "AndroidGenesGenesInOrder", MethodType.Getter)]
    public static class ASE_GeneRegistrationPatch
    {
        private static bool genesAdded = false;
        
        [HarmonyPriority(Priority.First)]
        public static void Postfix(ref List<GeneDef> __result)
        {
            Log.Message("[ASE] GeneRegistrationPatch Postfix called. genesAdded=" + genesAdded + ", result null=" + (__result == null));
            
            // Добавляем гены только один раз
            if (genesAdded || __result == null)
            {
                return;
            }
            
            // Собираем все наши AndroidGeneDef с префиксом ASE_
            var aseGenes = DefDatabase<GeneDef>.AllDefs
                .Where(g => g.defName.StartsWith("ASE_") && g is VREAndroids.AndroidGeneDef)
                .ToList();
            
            int addedCount = 0;
            foreach (var gene in aseGenes)
            {
                // Проверяем что гена ещё нет в списке (по defName, не по ссылке)
                if (!__result.Any(g => g.defName == gene.defName))
                {
                    __result.Add(gene);
                    addedCount++;
                }
            }
            
            if (addedCount > 0)
            {
                Log.Message("[ASE] Added " + addedCount + " ASE genes to android gene list. Total ASE genes: " + aseGenes.Count);
            }
            
            genesAdded = true;
        }
        
        /// <summary>
        /// Сброс флага при перезагрузке игры
        /// </summary>
        public static void Reset()
        {
            genesAdded = false;
        }
    }
}

