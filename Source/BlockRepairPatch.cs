using HarmonyLib;
using Verse;
using RimWorld;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Harmony патч для блокировки ремонта андроидов с определёнными генами
    /// Патчим VREAndroids.JobDriver_RepairAndroid.CanRepairAndroid
    /// </summary>
    [HarmonyPatch]
    public static class BlockRepairPatch
    {
        private static bool Prepare()
        {
            // Проверяем, существует ли класс VREAndroids.JobDriver_RepairAndroid
            return AccessTools.TypeByName("VREAndroids.JobDriver_RepairAndroid") != null;
        }

        private static System.Reflection.MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("VREAndroids.JobDriver_RepairAndroid");
            return AccessTools.Method(type, "CanRepairAndroid");
        }

        /// <summary>
        /// Постфикс: если андроид имеет ген, блокирующий ремонт, возвращаем false
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(Pawn android, ref bool __result)
        {
            if (!__result)
                return;

            if (android == null || android.genes == null)
                return;

            // Проверяем наличие генов, блокирующих ремонт
            foreach (Gene gene in android.genes.GenesListForReading)
            {
                if (gene.def.defName == "ASE_HandmadeEnergyBooster")
                {
                    __result = false;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Дополнительный патч для WorkGiver_RepairAndroid чтобы андроид не появлялся в списке для ремонта
    /// </summary>
    [HarmonyPatch]
    public static class BlockRepairWorkGiverPatch
    {
        private static bool Prepare()
        {
            return AccessTools.TypeByName("VREAndroids.WorkGiver_RepairAndroid") != null;
        }

        private static System.Reflection.MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("VREAndroids.WorkGiver_RepairAndroid");
            return AccessTools.Method(type, "HasJobOnThing");
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Thing t, ref bool __result)
        {
            if (!__result)
                return;

            Pawn android = t as Pawn;
            if (android == null || android.genes == null)
                return;

            foreach (Gene gene in android.genes.GenesListForReading)
            {
                if (gene.def.defName == "ASE_HandmadeEnergyBooster")
                {
                    __result = false;
                    return;
                }
            }
        }
    }
}
