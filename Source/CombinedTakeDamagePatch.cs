using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Объединённый Harmony патч для Thing.TakeDamage
    /// Обрабатывает: модификацию ranged урона (Combat Data Logger), логирование ranged урона, логирование входящего урона
    /// </summary>
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public static class CombinedTakeDamagePatch
    {
        // Структуры для логирования
        private struct RangedDamageLogData
        {
            public bool shouldLog;
            public string attackerName;
            public string targetName;
            public float baseDamage;
            public float damageMultiplier;
            public float expectedDamage;
            public string damageType;
            public string weaponName;
            public bool isRanged;
        }
        
        private struct IncomingDamageLogData
        {
            public bool shouldLog;
            public string attackerName;
            public string targetName;
            public float baseDamage;
            public float incomingFactor;
            public float expectedAfterFactor;
            public string damageType;
            public string effectType;
            public string percentStr;
        }
        
        [System.ThreadStatic]
        private static RangedDamageLogData rangedLogData;
        
        [System.ThreadStatic]
        private static IncomingDamageLogData incomingLogData;
        
        /// <summary>
        /// Проверяет, является ли урон ranged (Bullet, Arrow, Flame)
        /// </summary>
        private static bool IsRangedDamage(DamageInfo dinfo)
        {
            if (dinfo.Def == null)
                return false;
            
            string defName = dinfo.Def.defName;
            return defName == "Bullet" || defName == "Arrow" || defName == "Flame";
        }
        
        private static bool ShouldLogRanged()
        {
            return AndroidSubroutinesExpandedMod.Settings != null && 
                   AndroidSubroutinesExpandedMod.Settings.enableGeneralLogging;
        }
        
        private static bool ShouldLogIncoming()
        {
            return AndroidSubroutinesExpandedMod.Settings != null && 
                   AndroidSubroutinesExpandedMod.Settings.enableIncomingDamageLogging;
        }
        
        /// <summary>
        /// Prefix: модифицирует урон и собирает данные для логирования
        /// </summary>
        [HarmonyPrefix]
        public static void Prefix(Thing __instance, ref DamageInfo dinfo)
        {
            try
            {
                // Инициализация структур логирования
                rangedLogData = new RangedDamageLogData();
                rangedLogData.shouldLog = false;
                rangedLogData.isRanged = false;
                
                incomingLogData = new IncomingDamageLogData();
                incomingLogData.shouldLog = false;
                
                bool isRanged = IsRangedDamage(dinfo);
                
                // === 1. Модификация ranged урона (Combat Data Logger) ===
                if (isRanged)
                {
                    Pawn attacker = dinfo.Instigator as Pawn;
                    
                    if (attacker != null && attacker.genes != null)
                    {
                        Gene_CombatDataLogger combatDataLogger = null;
                        foreach (Gene gene in attacker.genes.GenesListForReading)
                        {
                            Gene_CombatDataLogger logger = gene as Gene_CombatDataLogger;
                            if (logger != null)
                            {
                                combatDataLogger = logger;
                                break;
                            }
                        }
                        
                        if (combatDataLogger != null && combatDataLogger.killCount > 0)
                        {
                            float damageMultiplier = 1.0f + (combatDataLogger.killCount * 0.01f);
                            float originalAmount = dinfo.Amount;
                            float modifiedAmount = originalAmount * damageMultiplier;
                            
                            DamageInfo modifiedDinfo = new DamageInfo(
                                def: dinfo.Def,
                                amount: modifiedAmount,
                                armorPenetration: dinfo.ArmorPenetrationInt,
                                angle: dinfo.Angle,
                                instigator: dinfo.Instigator,
                                hitPart: dinfo.HitPart,
                                weapon: dinfo.Weapon
                            );
                            
                            dinfo = modifiedDinfo;
                            
                            Log.Message("[ASE] RangedDamageModifierPatch: " + attacker.LabelShort + 
                                       " applied +" + (combatDataLogger.killCount) + "% damage multiplier (Combat Data Logger: " + combatDataLogger.killCount + " kills) | " +
                                       "Original: " + originalAmount.ToString("F1") + 
                                       " -> Modified: " + modifiedAmount.ToString("F1") + " (×" + damageMultiplier.ToString("F2") + ")");
                        }
                    }
                }
                
                // === 2. Сбор данных для логирования ranged урона ===
                if (isRanged && ShouldLogRanged())
                {
                    rangedLogData.isRanged = true;
                    
                    Pawn targetPawn = __instance as Pawn;
                    if (targetPawn != null)
                    {
                        Pawn attacker = dinfo.Instigator as Pawn;
                        if (attacker != null)
                        {
                            rangedLogData.shouldLog = true;
                            rangedLogData.attackerName = attacker.LabelShort;
                            rangedLogData.targetName = targetPawn.LabelShort;
                            rangedLogData.baseDamage = dinfo.Amount;
                            rangedLogData.damageType = dinfo.Def.defName;
                            
                            float rangedDamageMultiplier = 1f;
                            StatDef rangedDamageStat = DefDatabase<StatDef>.GetNamedSilentFail("RangedWeapon_DamageMultiplier");
                            if (rangedDamageStat != null)
                            {
                                rangedDamageMultiplier = attacker.GetStatValue(rangedDamageStat, true);
                            }
                            rangedLogData.damageMultiplier = rangedDamageMultiplier;
                            rangedLogData.expectedDamage = rangedLogData.baseDamage * rangedDamageMultiplier;
                            
                            rangedLogData.weaponName = "Unknown";
                            if (attacker.equipment != null && attacker.equipment.Primary != null)
                            {
                                rangedLogData.weaponName = attacker.equipment.Primary.Label;
                            }
                        }
                    }
                }
                
                // === 3. Сбор данных для логирования входящего урона ===
                if (ShouldLogIncoming())
                {
                    Pawn pawn = __instance as Pawn;
                    if (pawn != null)
                    {
                        float incomingDamageFactor = pawn.GetStatValue(StatDefOf.IncomingDamageFactor, true);
                        
                        if (System.Math.Abs(incomingDamageFactor - 1f) > 0.01f)
                        {
                            incomingLogData.shouldLog = true;
                            incomingLogData.targetName = pawn.LabelShort;
                            incomingLogData.baseDamage = dinfo.Amount;
                            incomingLogData.incomingFactor = incomingDamageFactor;
                            incomingLogData.expectedAfterFactor = dinfo.Amount * incomingDamageFactor;
                            incomingLogData.damageType = dinfo.Def.defName;
                            incomingLogData.effectType = incomingDamageFactor < 1f ? "REDUCED" : "INCREASED";
                            
                            float percentChange = (incomingDamageFactor - 1f) * 100f;
                            incomingLogData.percentStr = percentChange >= 0 ? "+" + percentChange.ToString("F0") + "%" : percentChange.ToString("F0") + "%";
                            
                            incomingLogData.attackerName = "Unknown";
                            if (dinfo.Instigator != null)
                            {
                                Pawn attackerPawn = dinfo.Instigator as Pawn;
                                if (attackerPawn != null)
                                {
                                    incomingLogData.attackerName = attackerPawn.LabelShort;
                                }
                                else
                                {
                                    incomingLogData.attackerName = dinfo.Instigator.Label;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("[ASE] CombinedTakeDamagePatch Prefix error: " + ex.ToString());
            }
        }
        
        /// <summary>
        /// Postfix: логирует результаты после применения урона
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(Thing __instance, DamageWorker.DamageResult __result)
        {
            try
            {
                // === Логирование ranged урона ===
                if (rangedLogData.shouldLog && rangedLogData.isRanged)
                {
                    Pawn targetPawn = __instance as Pawn;
                    if (targetPawn != null)
                    {
                        float actualDamage = 0f;
                        if (__result != null)
                        {
                            actualDamage = __result.totalDamageDealt;
                        }
                        
                        Log.Message("[ASE RANGED] " + rangedLogData.attackerName + " -> " + rangedLogData.targetName + 
                                   " | Weapon: " + rangedLogData.weaponName +
                                   " | Base Damage: " + rangedLogData.baseDamage.ToString("F1") +
                                   " | Damage Type: " + rangedLogData.damageType +
                                   " | Damage Multiplier: x" + rangedLogData.damageMultiplier.ToString("F2") +
                                   " | Expected Damage: " + rangedLogData.expectedDamage.ToString("F1") +
                                   " | Actual Damage: " + actualDamage.ToString("F1"));
                        
                        // Логируем статистики стрелка
                        Pawn attacker = Find.WorldPawns.AllPawnsAliveOrDead.FirstOrDefault(p => p.LabelShort == rangedLogData.attackerName);
                        if (attacker == null && rangedLogData.attackerName != "Unknown")
                        {
                            if (Find.CurrentMap != null)
                            {
                                attacker = Find.CurrentMap.mapPawns.AllPawns.FirstOrDefault(p => p.LabelShort == rangedLogData.attackerName);
                            }
                        }
                        
                        if (attacker != null)
                        {
                            float shootingAccuracy = attacker.GetStatValue(StatDefOf.ShootingAccuracyPawn, true);
                            Log.Message("[ASE RANGED STATS] " + rangedLogData.attackerName + 
                                       " | Shooting Accuracy: " + (shootingAccuracy * 100f).ToString("F1") + "%" +
                                       " | Damage Multiplier: x" + rangedLogData.damageMultiplier.ToString("F2"));
                        }
                    }
                }
                
                // === Логирование входящего урона ===
                if (incomingLogData.shouldLog)
                {
                    float actualDamage = 0f;
                    if (__result != null)
                    {
                        actualDamage = __result.totalDamageDealt;
                    }
                    
                    Log.Message("[ASE DMG " + incomingLogData.effectType + "] " + incomingLogData.attackerName + " -> " + incomingLogData.targetName + 
                               " | Weapon: " + incomingLogData.baseDamage.ToString("F1") + 
                               " | ×" + incomingLogData.incomingFactor.ToString("F2") + " (" + incomingLogData.percentStr + ")" +
                               " = " + incomingLogData.expectedAfterFactor.ToString("F1") + 
                               " | After Armor: " + actualDamage.ToString("F1") +
                               " | " + incomingLogData.damageType);
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("[ASE] CombinedTakeDamagePatch Postfix error: " + ex.ToString());
            }
        }
    }
}

