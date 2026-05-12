using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Объединённый Harmony патч для PreApplyDamage
    /// Prefix: блокирующие эффекты (Anti-Insectoid Plates, Reactive Armor, Deflector System)
    /// Postfix: эффекты которые должны сработать всегда (Resilience, Toxic Discharge)
    /// </summary>
    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    public static class CombinedDamagePatch
    {
        // Флаг для предотвращения рекурсии
        private static bool isProcessing = false;
        
        // Кэшируем поле pawn для производительности
        private static FieldInfo pawnField = null;
        
        // Константы для Toxic Discharge
        private const float TOXIC_CLOUD_RADIUS = 3f;
        private const int TOXIC_CLOUD_DURATION = 100;
        private const float TOXIC_DAMAGE_AMOUNT = 5f;
        private const float TOXIC_BUILDUP_SEVERITY = 0.15f;
        
        // === Методы для проверки настроек ===
        private static bool ShouldLogReactiveArmor()
        {
            return AndroidSubroutinesExpandedMod.Settings != null && 
                   AndroidSubroutinesExpandedMod.Settings.enableReactiveArmorLogging;
        }
        
        private static bool ShouldLogDeflectorSystem()
        {
            return AndroidSubroutinesExpandedMod.Settings != null && 
                   AndroidSubroutinesExpandedMod.Settings.enableDeflectorSystemLogging;
        }
        
        private static bool ShouldLogAntiInsectoidPlates()
        {
            return AndroidSubroutinesExpandedMod.Settings != null && 
                   AndroidSubroutinesExpandedMod.Settings.enableAntiInsectoidPlatesLogging;
        }
        
        private static bool ShouldShowReactiveArmorMessages()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || 
                   AndroidSubroutinesExpandedMod.Settings.enableReactiveArmorMessages;
        }
        
        private static bool ShouldShowDeflectorSystemMessages()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || 
                   AndroidSubroutinesExpandedMod.Settings.enableDeflectorSystemMessages;
        }
        
        private static bool ShouldShowAntiInsectoidPlatesMessages()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || 
                   AndroidSubroutinesExpandedMod.Settings.enableAntiInsectoidPlatesMessages;
        }
        
        /// <summary>
        /// Находит любой hediff Anti-Insectoid Plates (MkI, MkII или MkIII)
        /// </summary>
        private static Hediff_AntiInsectoidPlates FindAnyPlatesHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null) return null;
            
            string[] hediffNames = new string[] 
            { 
                "ASE_AntiInsectoidPlates_MkI", 
                "ASE_AntiInsectoidPlates_MkII", 
                "ASE_AntiInsectoidPlates_MkIII" 
            };
            
            foreach (string hediffName in hediffNames)
            {
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffName);
                if (hediffDef != null)
                {
                    Hediff_AntiInsectoidPlates plates = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) as Hediff_AntiInsectoidPlates;
                    if (plates != null)
                    {
                        return plates;
                    }
                }
            }
            
            return null;
        }
        
        static CombinedDamagePatch()
        {
            try
            {
                // Получаем поле pawn через рефлексию один раз
                pawnField = typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
                if (pawnField == null)
                {
                    Log.Error("[ASE] CombinedDamagePatch: Failed to find 'pawn' field");
                }
                else
                {
                    Log.Message("[ASE] CombinedDamagePatch: Loaded successfully");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("[ASE] CombinedDamagePatch: Error - " + ex.ToString());
            }
        }
        
        /// <summary>
        /// Prefix: блокирующие эффекты (Anti-Insectoid Plates, Reactive Armor, Deflector System)
        /// </summary>
        [HarmonyPrefix]
        public static bool Prefix(Pawn_HealthTracker __instance, DamageInfo dinfo, ref bool absorbed)
        {
            try
            {
                // ЗАЩИТА ОТ РЕКУРСИИ
                if (isProcessing) return true;
                
                if (pawnField == null) return true;
                Pawn pawn = pawnField.GetValue(__instance) as Pawn;
                if (pawn == null || pawn.genes == null) return true;
                
                Pawn attacker = dinfo.Instigator as Pawn;
                if (attacker == null) return true;
                
                isProcessing = true;
                
                // ПРИОРИТЕТ 1: Anti-Insectoid Plates (только для melee урона)
                if (dinfo.Def != DamageDefOf.Bullet && dinfo.Def.defName != "Arrow" && dinfo.Def.defName != "Flame")
                {
                    Hediff_AntiInsectoidPlates plates = FindAnyPlatesHediff(pawn);
                    
                    if (plates != null && plates.Severity < 0f)
                    {
                        Log.Warning("[ASE] Anti-Insectoid Plates had negative severity (" + plates.Severity.ToString("F1") + "), resetting to 0");
                        plates.Severity = 0f;
                    }
                    
                    if (plates != null && plates.Severity > 0f)
                    {
                        float damageAmount = dinfo.Amount;
                        float armorBefore = plates.Severity;
                        string attackerName = attacker != null ? attacker.LabelShort : "Unknown";
                        
                        if (damageAmount >= plates.Severity)
                        {
                            plates.Severity = 0f;
                            
                            if (ShouldLogAntiInsectoidPlates())
                            {
                                Log.Message("[ASE PLATES DESTROYED] " + attackerName + " -> " + pawn.LabelShort + 
                                           " | Incoming: " + damageAmount.ToString("F1") + " " + dinfo.Def.defName +
                                           " | Armor: " + armorBefore.ToString("F0") + " -> 0" +
                                           " | Blocked: " + damageAmount.ToString("F1") + " (100%)" +
                                           " | Status: DESTROYED");
                            }
                            
                            if (ShouldShowAntiInsectoidPlatesMessages() && pawn.Faction == Faction.OfPlayer)
                            {
                                Messages.Message(pawn.LabelShort + " anti-insectoid plates destroyed! (blocked " + damageAmount.ToString("F0") + " dmg)", 
                                               pawn, MessageTypeDefOf.NegativeEvent);
                            }
                            
                            absorbed = true;
                            isProcessing = false;
                            return false;
                        }
                        else
                        {
                            plates.Severity -= damageAmount;
                            if (plates.Severity < 0f) plates.Severity = 0f;
                            float armorAfter = plates.Severity;
                            
                            if (ShouldLogAntiInsectoidPlates())
                            {
                                Log.Message("[ASE PLATES ABSORB] " + attackerName + " -> " + pawn.LabelShort + 
                                           " | Incoming: " + damageAmount.ToString("F1") + " " + dinfo.Def.defName +
                                           " | Armor: " + armorBefore.ToString("F0") + " -> " + armorAfter.ToString("F0") +
                                           " | Blocked: " + damageAmount.ToString("F1") + " (100%)" +
                                           " | Remaining: " + armorAfter.ToString("F0") + " HP");
                            }
                            
                            if (ShouldShowAntiInsectoidPlatesMessages() && pawn.Faction == Faction.OfPlayer)
                            {
                                Messages.Message(pawn.LabelShort + " plates: -" + damageAmount.ToString("F0") + 
                                               " (" + armorAfter.ToString("F0") + " remaining)", 
                                               pawn, MessageTypeDefOf.NeutralEvent);
                            }
                            
                            absorbed = true;
                            isProcessing = false;
                            return false;
                        }
                    }
                }
                
                // ПРИОРИТЕТ 2: Reactive Armor (только для melee урона)
                if (dinfo.Def != DamageDefOf.Bullet && dinfo.Def.defName != "Arrow" && dinfo.Def.defName != "Flame")
                {
                    bool hasReactiveArmor = false;
                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        if (gene.def.defName == "ASE_ReactiveArmor")
                        {
                            hasReactiveArmor = true;
                            break;
                        }
                    }
                    
                    if (hasReactiveArmor)
                    {
                        float reflectedDamage = dinfo.Amount;
                        string attackerName = attacker != null ? attacker.LabelShort : "Unknown";
                        
                        DamageInfo reflectDinfo = new DamageInfo(
                            def: dinfo.Def,
                            amount: reflectedDamage,
                            armorPenetration: dinfo.ArmorPenetrationInt,
                            angle: 180f,
                            instigator: pawn,
                            hitPart: null,
                            weapon: null
                        );
                        
                        attacker.TakeDamage(reflectDinfo);
                        
                        if (ShouldLogReactiveArmor())
                        {
                            Log.Message("[ASE REACTIVE REFLECT] " + attackerName + " -> " + pawn.LabelShort + 
                                       " | Incoming: " + reflectedDamage.ToString("F1") + " " + dinfo.Def.defName +
                                       " | Both take damage!" +
                                       " | Target takes: " + reflectedDamage.ToString("F1") + 
                                       " | Attacker takes: " + reflectedDamage.ToString("F1"));
                        }
                        
                        if (ShouldShowReactiveArmorMessages() && pawn.Faction == Faction.OfPlayer)
                        {
                            Messages.Message(pawn.LabelShort + " reflects " + reflectedDamage.ToString("F0") + " dmg -> " + attackerName + " (both hit!)", 
                                           pawn, MessageTypeDefOf.NeutralEvent);
                        }
                        
                        isProcessing = false;
                        return true; // Продолжаем применение урона к цели
                    }
                }
                
                // ПРИОРИТЕТ 3: Deflector System (только для ranged урона)
                if (dinfo.Def.defName == "Bullet" || dinfo.Def.defName == "Arrow" || dinfo.Def.defName == "Flame")
                {
                    bool hasDeflectorSystem = false;
                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        if (gene.def.defName == "ASE_DeflectorSystem")
                        {
                            hasDeflectorSystem = true;
                            break;
                        }
                    }
                    
                    if (hasDeflectorSystem)
                    {
                        float incomingDamage = dinfo.Amount;
                        string attackerName = attacker != null ? attacker.LabelShort : "Unknown";
                        
                        float chance = Rand.Value;
                        float chancePercent = chance * 100f;
                        
                        if (chance < 0.5f) 
                        {
                            DamageInfo reflectDinfo = new DamageInfo(
                                def: dinfo.Def,
                                amount: incomingDamage,
                                armorPenetration: dinfo.ArmorPenetrationInt,
                                angle: 180f,
                                instigator: pawn,
                                hitPart: null,
                                weapon: null
                            );
                            
                            attacker.TakeDamage(reflectDinfo);
                            
                            if (ShouldLogDeflectorSystem())
                            {
                                Log.Message("[ASE DEFLECTOR SUCCESS] " + attackerName + " -> " + pawn.LabelShort + 
                                           " | Incoming: " + incomingDamage.ToString("F1") + " " + dinfo.Def.defName +
                                           " | Roll: " + chancePercent.ToString("F0") + "% (need <50%)" +
                                           " | Reflected: " + incomingDamage.ToString("F1") + " -> " + attackerName +
                                           " | Target took: 0");
                            }
                            
                            if (ShouldShowDeflectorSystemMessages() && pawn.Faction == Faction.OfPlayer)
                            {
                                Messages.Message(pawn.LabelShort + " deflects " + incomingDamage.ToString("F0") + " ranged dmg -> " + attackerName, 
                                               pawn, MessageTypeDefOf.NeutralEvent);
                            }
                            
                            absorbed = true;
                            isProcessing = false;
                            return false;
                        }
                        else
                        {
                            if (ShouldLogDeflectorSystem())
                            {
                                Log.Message("[ASE DEFLECTOR FAILED] " + attackerName + " -> " + pawn.LabelShort + 
                                           " | Incoming: " + incomingDamage.ToString("F1") + " " + dinfo.Def.defName +
                                           " | Roll: " + chancePercent.ToString("F0") + "% (need <50%)" +
                                           " | Result: DAMAGE PASSES THROUGH");
                            }
                        }
                    }
                }
                
                isProcessing = false;
                return true; // Продолжаем применение урона
            }
            catch (System.Exception ex)
            {
                isProcessing = false;
                Log.Error("[ASE] CombinedDamagePatch Prefix error: " + ex.ToString());
                return true; // В случае ошибки продолжаем применение урона
            }
        }
        
        /// <summary>
        /// Postfix: эффекты которые должны сработать всегда (Resilience, Toxic Discharge)
        /// Выполняется даже если урон был заблокирован
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(Pawn_HealthTracker __instance, DamageInfo dinfo, ref bool absorbed)
        {
            try
            {
                if (pawnField == null) return;
                Pawn pawn = pawnField.GetValue(__instance) as Pawn;
                if (pawn == null || pawn.genes == null) return;
                
                // Игнорируем урон от самого себя
                if (dinfo.Instigator == pawn) return;
                
                // Resilience: увеличиваем броню от каждого удара
                try
                {
                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        Gene_Resilience resilience = gene as Gene_Resilience;
                        if (resilience != null)
                        {
                            resilience.OnDamageTaken();
                            break;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("[ASE] CombinedDamagePatch Resilience error: " + ex.ToString());
                }
                
                // Toxic Discharge: создаём токсичное облако при получении урона
                try
                {
                    if (pawn.Map == null) return;
                    
                    Gene_ToxicDischarge toxicDischarge = null;
                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        Gene_ToxicDischarge td = gene as Gene_ToxicDischarge;
                        if (td != null)
                        {
                            toxicDischarge = td;
                            break;
                        }
                    }
                    
                    if (toxicDischarge != null && toxicDischarge.HasCharges())
                    {
                        if (toxicDischarge.TryConsumeCharge())
                        {
                            CreateToxicCloud(pawn, TOXIC_CLOUD_RADIUS, TOXIC_CLOUD_DURATION);
                            
                            Log.Message("[ASE] Toxic Discharge: " + pawn.LabelShort + 
                                       " released toxic cloud (charges remaining: " + 
                                       (toxicDischarge.HasCharges() ? "yes" : "no") + ")");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("[ASE] CombinedDamagePatch Toxic Discharge error: " + ex.ToString());
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("[ASE] CombinedDamagePatch Postfix error: " + ex.ToString());
            }
        }
        
        /// <summary>
        /// Создаёт токсичное облако вокруг пешки
        /// </summary>
        private static void CreateToxicCloud(Pawn pawn, float radius, int duration)
        {
            if (pawn == null || pawn.Map == null) return;
            
            // Визуальный эффект
            GenExplosion.DoExplosion(
                center: pawn.Position,
                map: pawn.Map,
                radius: radius,
                damType: DamageDefOf.ToxGas,
                instigator: pawn,
                damAmount: 0,
                armorPenetration: 0f,
                explosionSound: null,
                weapon: null,
                projectile: null,
                intendedTarget: null,
                postExplosionSpawnThingDef: null,
                postExplosionSpawnChance: 0f,
                postExplosionSpawnThingCount: 0,
                postExplosionGasType: null,
                applyDamageToExplosionCellsNeighbors: false,
                preExplosionSpawnThingDef: null,
                preExplosionSpawnChance: 0f,
                preExplosionSpawnThingCount: 0,
                chanceToStartFire: 0f,
                damageFalloff: true,
                direction: null,
                ignoredThings: null,
                affectedAngle: null,
                doVisualEffects: true,
                propagationSpeed: 1f,
                excludeRadius: 0f,
                doSoundEffects: true,
                screenShakeFactor: 0f
            );
            
            // Находим всех в радиусе и применяем урон и эффекты
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, radius, true))
            {
                Pawn target = thing as Pawn;
                if (target == null || target == pawn || target.Dead) continue;
                
                // Применяем токсичный урон
                DamageInfo toxicDamage = new DamageInfo(
                    def: DamageDefOf.ToxGas,
                    amount: TOXIC_DAMAGE_AMOUNT,
                    instigator: pawn,
                    hitPart: null
                );
                target.TakeDamage(toxicDamage);
                
                // Применяем hediff ToxicBuildup
                HediffDef toxicBuildupDef = DefDatabase<HediffDef>.GetNamedSilentFail("ToxicBuildup");
                if (toxicBuildupDef != null)
                {
                    Hediff existingToxic = target.health.hediffSet.GetFirstHediffOfDef(toxicBuildupDef);
                    if (existingToxic != null)
                    {
                        existingToxic.Severity = System.Math.Min(1f, existingToxic.Severity + TOXIC_BUILDUP_SEVERITY);
                    }
                    else
                    {
                        Hediff toxicBuildup = HediffMaker.MakeHediff(toxicBuildupDef, target);
                        toxicBuildup.Severity = TOXIC_BUILDUP_SEVERITY;
                        target.health.AddHediff(toxicBuildup);
                    }
                }
            }
        }
    }
}

