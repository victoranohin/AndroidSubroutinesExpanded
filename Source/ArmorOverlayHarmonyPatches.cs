using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    [HarmonyPatch(typeof(StatWorker), "GetValue", new System.Type[] { typeof(StatRequest), typeof(bool) })]
    public static class StatWorker_GetValue_ArmorOverlay_Patch
    {
        public static void Postfix(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float __result)
        {
            try
            {
                // Проверяем, что это запрос для пауна
                if (req == null || req.Thing == null)
                    return;

                Pawn pawn = req.Thing as Pawn;
                if (pawn == null || !pawn.IsAndroid())
                    return;

                // Получаем StatDef через рефлексию
                var statField = typeof(StatWorker).GetField("stat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (statField == null)
                    return;

                StatDef stat = statField.GetValue(__instance) as StatDef;
                if (stat == null)
                    return;

                // ПРИМЕЧАНИЕ: Большинство статов в XML через statOffsets/statFactors
                // ИСКЛЮЧЕНИЕ: MarketValue для пешек не работает через XML statFactors и hediff.statOffsets - нужен патч
                
                // Применяем бонусы к MarketValue для Golden и Diamond Overlay
                if (stat == StatDefOf.MarketValue)
                {
                    // Golden Overlay - +20 000
                    Hediff goldenHediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                        DefDatabase<HediffDef>.GetNamedSilentFail("ASE_GoldenOverlayHediff"));
                    if (goldenHediff != null && goldenHediff is Hediff_GoldenOverlay)
                    {
                        __result += 20000f;
                    }
                    
                    // Diamond Overlay - накопленная стоимость
                    Hediff diamondHediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                        DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DiamondOverlayHediff"));
                    if (diamondHediff != null && diamondHediff is Hediff_DiamondOverlay)
                    {
                        Hediff_DiamondOverlay diamondOverlay = diamondHediff as Hediff_DiamondOverlay;
                        __result += diamondOverlay.accumulatedValue;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("ArmorOverlayHarmonyPatches error: " + ex.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Tick")]
    public static class Pawn_Tick_AutoRepair_Patch
    {
        private static int tickCounter = 0;
        private const int REPAIR_TICK_INTERVAL = 500;
        private const int LIMB_REPAIR_TICK_INTERVAL = 5000;

        public static void Postfix(Pawn __instance)
        {
            try
            {
                // Проверяем, что это андроид
                if (__instance == null || !__instance.IsAndroid() || __instance.Dead)
                    return;

                // Проверяем наличие Auto-Repair Module
                bool hasAutoRepair = __instance.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamedSilentFail("ASE_AutoRepairModule"));
                if (!hasAutoRepair)
                    return;

                tickCounter++;

                // Восстановление повреждений
                if (tickCounter >= REPAIR_TICK_INTERVAL)
                {
                    tickCounter = 0;
                    RepairDamage(__instance);
                }

                // Восстановление отсутствующих частей тела
                if (tickCounter % LIMB_REPAIR_TICK_INTERVAL == 0)
                {
                    RepairMissingBodyParts(__instance);
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("AutoRepairHarmonyPatches error: " + ex.ToString());
            }
        }

        private static void RepairDamage(Pawn pawn)
        {
            if (pawn.health == null || pawn.health.hediffSet == null || pawn.health.hediffSet.hediffs == null)
                return;

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                Hediff_Injury injury = hediff as Hediff_Injury;
                if (injury != null && injury.Severity > 0)
                {
                    injury.Severity = System.Math.Max(0, injury.Severity - 1f);
                    break;
                }
            }
        }

        private static void RepairMissingBodyParts(Pawn pawn)
        {
            if (pawn.health == null || pawn.health.hediffSet == null)
                return;

            List<Hediff_MissingPart> missingParts = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
            if (missingParts == null || missingParts.Count == 0)
                return;

            HediffDef addedBodyPartDef = DefDatabase<HediffDef>.GetNamedSilentFail("AddedBodyPart");
            if (addedBodyPartDef == null)
                return;

            foreach (Hediff_MissingPart missingPart in missingParts)
            {
                if (missingPart != null && missingPart.Part != null)
                {
                    Hediff_AddedPart addedPart = (Hediff_AddedPart)HediffMaker.MakeHediff(
                        addedBodyPartDef,
                        pawn,
                        missingPart.Part);

                    if (addedPart != null)
                    {
                        pawn.health.RemoveHediff(missingPart);
                        pawn.health.AddHediff(addedPart);
                    }
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("GetGizmos")]
    public static class Pawn_GetGizmos_WallProtocol_Patch
    {
        public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            try
            {
                // Проверяем, что это андроид
                if (__instance == null || !__instance.IsAndroid())
                    return;

                // Проверяем наличие Wall Protocol
                bool hasWallProtocol = __instance.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamedSilentFail("ASE_WallProtocol"));
                if (!hasWallProtocol)
                    return;

                // Убираем кнопки каравана для Wall Protocol
                var gizmos = new List<Gizmo>(__result);
                gizmos.RemoveAll(g => {
                    Command_Action action = g as Command_Action;
                    return action != null && action.defaultLabel.Contains("Caravan");
                });
                __result = gizmos;
            }
            catch (System.Exception ex)
            {
                Log.Error("WallProtocolGizmosHarmonyPatches error: " + ex.ToString());
            }
        }
    }
}
