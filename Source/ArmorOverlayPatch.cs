using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Патч для CaravanFormingUtility.AllSendablePawns
    /// Исключает пешек с Wall Protocol из списка доступных для каравана
    /// </summary>
    [HarmonyPatch(typeof(CaravanFormingUtility), "AllSendablePawns")]
    public static class WallProtocolCaravanPatch
    {
        static WallProtocolCaravanPatch()
        {
            Log.Message("PATCH: WallProtocolCaravanPatch static constructor called");
        }

        public static void Postfix(ref List<Pawn> __result, Map map)
        {
            try
            {
                if (__result == null || __result.Count == 0) return;
                
                GeneDef wallProtocolDef = DefDatabase<GeneDef>.GetNamedSilentFail("ASE_WallProtocol");
                if (wallProtocolDef == null) return;
                
                // Фильтруем пешек с Wall Protocol
                int originalCount = __result.Count;
                __result = __result.Where(pawn => {
                    if (pawn == null || pawn.genes == null) return true;
                    bool hasWallProtocol = pawn.genes.HasActiveGene(wallProtocolDef);
                    if (hasWallProtocol)
                    {
                        Log.Message("[ASE] Wall Protocol: " + pawn.LabelShort + " excluded from caravan");
                    }
                    return !hasWallProtocol;
                }).ToList();
                
                if (__result.Count < originalCount)
                {
                    Log.Message("[ASE] Wall Protocol: Excluded " + (originalCount - __result.Count) + " pawn(s) from caravan list");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("[ASE] WallProtocolCaravanPatch error: " + ex.ToString());
            }
        }
    }
}
