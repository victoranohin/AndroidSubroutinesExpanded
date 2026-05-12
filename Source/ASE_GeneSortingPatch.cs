using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Patch to sort android genes properly by category.
    /// Does NOT add genes - VRE Androids handles that automatically for AndroidGeneDef.
    /// Only handles sorting to keep genes grouped by category.
    /// </summary>
    [HarmonyPatch(typeof(Utils), "AndroidGenesGenesInOrder", MethodType.Getter)]
    public static class ASE_GeneSortingPatch
    {
        private static bool sorted = false;
        
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ref List<GeneDef> __result)
        {
            // Only sort once to avoid constant re-sorting
            if (sorted || __result == null || __result.Count == 0)
            {
                return;
            }
            
            // Sort by: category priority (descending), category label, then display order within category
            __result.SortBy(
                (GeneDef x) => x.displayCategory != null ? -x.displayCategory.displayPriorityInXenotype : 0f,
                (GeneDef x) => x.displayCategory != null ? x.displayCategory.label : "",
                (GeneDef x) => x.displayOrderInCategory);
            
            sorted = true;
            Log.Message("[ASE] Gene sorting complete. Total genes: " + __result.Count);
        }
        
        /// <summary>
        /// Reset the sorted flag when game restarts (e.g., returning to main menu)
        /// </summary>
        public static void ResetSortFlag()
        {
            sorted = false;
        }
    }
}

