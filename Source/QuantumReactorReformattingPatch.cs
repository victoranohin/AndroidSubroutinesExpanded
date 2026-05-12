using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Harmony патч для увеличения времени форматирования андроида с Quantum Reactor в 3 раза
    /// Патчит метод VREAndroids.MentalState_Reformatting.TicksToRecoverFromReformatting
    /// </summary>
    [HarmonyPatch]
    public static class QuantumReactorReformattingPatch
    {
        private static GeneDef quantumReactorDef = null;
        private static bool initialized = false;

        static QuantumReactorReformattingPatch()
        {
            Log.Message("[ASE] QuantumReactorReformattingPatch: Static constructor called");
        }

        /// <summary>
        /// Определяем целевой метод динамически, так как VREAndroids загружается отдельно
        /// </summary>
        [HarmonyTargetMethod]
        public static System.Reflection.MethodBase TargetMethod()
        {
            // Ищем тип VREAndroids.MentalState_Reformatting
            System.Type reformattingType = null;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                reformattingType = assembly.GetType("VREAndroids.MentalState_Reformatting");
                if (reformattingType != null)
                    break;
            }

            if (reformattingType == null)
            {
                Log.Warning("[ASE] QuantumReactorReformattingPatch: VREAndroids.MentalState_Reformatting type not found");
                return null;
            }

            // Получаем метод TicksToRecoverFromReformatting
            var method = reformattingType.GetMethod("TicksToRecoverFromReformatting", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (method == null)
            {
                Log.Warning("[ASE] QuantumReactorReformattingPatch: TicksToRecoverFromReformatting method not found");
                return null;
            }

            Log.Message("[ASE] QuantumReactorReformattingPatch: Successfully found target method");
            return method;
        }

        /// <summary>
        /// Postfix: умножаем время форматирования на 3, если у андроида есть Quantum Reactor
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref int __result)
        {
            if (pawn == null)
                return;

            // Ленивая инициализация GeneDef
            if (!initialized)
            {
                quantumReactorDef = DefDatabase<GeneDef>.GetNamedSilentFail("ASE_ExperimentalQuantumReactor");
                initialized = true;
            }

            if (quantumReactorDef == null)
                return;

            // Проверяем, есть ли у пешки ген Quantum Reactor
            if (pawn.genes != null && pawn.genes.HasActiveGene(quantumReactorDef))
            {
                // Умножаем время на 3 (300% reformatting time)
                __result = __result * 3;
                
                if (AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableGeneralLogging)
                {
                    Log.Message("[ASE] QuantumReactorReformattingPatch: Applied 3x reformatting time for " + pawn.LabelShort + " (new ticks: " + __result + ")");
                }
            }
        }
    }

    /// <summary>
    /// Дополнительный патч для PreStart, чтобы убедиться что stance устанавливается правильно
    /// </summary>
    [HarmonyPatch]
    public static class QuantumReactorPreStartPatch
    {
        private static GeneDef quantumReactorDef = null;
        private static bool initialized = false;

        static QuantumReactorPreStartPatch()
        {
            Log.Message("[ASE] QuantumReactorPreStartPatch: Static constructor called");
        }

        [HarmonyTargetMethod]
        public static System.Reflection.MethodBase TargetMethod()
        {
            System.Type reformattingType = null;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                reformattingType = assembly.GetType("VREAndroids.MentalState_Reformatting");
                if (reformattingType != null)
                    break;
            }

            if (reformattingType == null)
            {
                Log.Warning("[ASE] QuantumReactorPreStartPatch: VREAndroids.MentalState_Reformatting type not found");
                return null;
            }

            var method = reformattingType.GetMethod("PreStart", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (method == null)
            {
                Log.Warning("[ASE] QuantumReactorPreStartPatch: PreStart method not found");
                return null;
            }

            Log.Message("[ASE] QuantumReactorPreStartPatch: Successfully found target method");
            return method;
        }

        [HarmonyPostfix]
        public static void Postfix(MentalState __instance)
        {
            if (__instance == null || __instance.pawn == null)
                return;

            if (!initialized)
            {
                quantumReactorDef = DefDatabase<GeneDef>.GetNamedSilentFail("ASE_ExperimentalQuantumReactor");
                initialized = true;
            }

            if (quantumReactorDef == null)
                return;

            if (__instance.pawn.genes != null && __instance.pawn.genes.HasActiveGene(quantumReactorDef))
            {
                if (__instance.pawn.stances != null && __instance.pawn.stances.curStance != null)
                {
                    var stanceType = __instance.pawn.stances.curStance.GetType();
                    if (stanceType.Name == "Stance_Stand")
                    {
                        var ticksLeftField = stanceType.GetField("ticksLeft", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (ticksLeftField != null)
                        {
                            int currentTicks = (int)ticksLeftField.GetValue(__instance.pawn.stances.curStance);
                            int newTicks = currentTicks * 3;
                            ticksLeftField.SetValue(__instance.pawn.stances.curStance, newTicks);
                            
                            if (AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableGeneralLogging)
                            {
                                Log.Message("[ASE] QuantumReactorPreStartPatch: Extended stance for " + __instance.pawn.LabelShort + " from " + currentTicks + " to " + newTicks + " ticks");
                            }
                        }
                    }
                }
            }
        }
    }
}

