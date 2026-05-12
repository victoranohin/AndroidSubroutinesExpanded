using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Объединённый Harmony патч для Verb_MeleeAttackDamage.DamageInfosToApply
    /// Обрабатывает: логирование melee урона, Heated Circuits (burn + ignite), Cryo Strike Protocol (frostbite + hypothermia)
    /// </summary>
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage))]
    [HarmonyPatch("DamageInfosToApply")]
    public static class CombinedMeleeDamagePatch
    {
        /// <summary>
        /// Postfix: логирует урон и применяет эффекты генов
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(Verb_MeleeAttackDamage __instance, LocalTargetInfo target, ref IEnumerable<DamageInfo> __result)
        {
            try
            {
                Pawn attacker = __instance.CasterPawn;
                if (attacker == null)
                    return;
                
                Pawn targetPawn = target.Thing as Pawn;
                if (targetPawn == null || targetPawn.Dead)
                    return;
                
                // === 1. Логирование melee урона ===
                if (__result != null)
                {
                    List<DamageInfo> damageList = __result.ToList();
                    float totalDamage = 0f;
                    
                    foreach (DamageInfo dinfo in damageList)
                    {
                        totalDamage = totalDamage + dinfo.Amount;
                        Log.Message("[ASE MELEE] " + attacker.LabelShort + " -> " + targetPawn.LabelShort + 
                                    " | Damage: " + dinfo.Amount + " (" + dinfo.Def.label + ")" +
                                    " | Armor Penetration: " + dinfo.ArmorPenetrationInt);
                    }
                    
                    if (damageList.Count > 1)
                    {
                        Log.Message("[ASE MELEE] Total damage from attack: " + totalDamage);
                    }
                    
                    float meleeDamageMultiplier = attacker.GetStatValue(StatDefOf.MeleeDamageFactor);
                    float meleeHitChance = attacker.GetStatValue(StatDefOf.MeleeHitChance);
                    
                    Log.Message("[ASE MELEE STATS] " + attacker.LabelShort + 
                                " | Damage Factor: x" + meleeDamageMultiplier.ToString("F2") + 
                                " | Hit Chance: " + (meleeHitChance * 100f).ToString("F1") + "%");
                }
                
                // === 2. Heated Circuits: burn + ignite ===
                if (attacker.genes != null && attacker.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamedSilentFail("ASE_HeatedCircuits")))
                {
                    try
                    {
                        // Apply burn damage
                        HediffDef burnDef = DefDatabase<HediffDef>.GetNamedSilentFail("Burn");
                        if (burnDef != null)
                        {
                            BodyPartRecord bodyPart = targetPawn.health.hediffSet.GetNotMissingParts().RandomElement();
                            if (bodyPart != null)
                            {
                                Hediff burn = HediffMaker.MakeHediff(burnDef, targetPawn, bodyPart);
                                burn.Severity = 0.20f;
                                targetPawn.health.AddHediff(burn);
                                
                                Log.Message("[ASE] Heated Circuits: " + attacker.LabelShort + " attacked " + targetPawn.LabelShort + 
                                            " -> Applied burn (severity: " + burn.Severity + ") to " + bodyPart.Label);
                            }
                        }
                        
                        // Try to ignite the target - 30% chance
                        if (targetPawn.Spawned && targetPawn.Map != null && Rand.Chance(0.3f))
                        {
                            DamageInfo flameDamage = new DamageInfo(
                                DamageDefOf.Flame,
                                amount: 1f,
                                instigator: attacker,
                                hitPart: null
                            );
                            targetPawn.TakeDamage(flameDamage);
                            
                            Log.Message("[ASE] Heated Circuits: " + attacker.LabelShort + " ignited " + targetPawn.LabelShort);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error("[ASE] CombinedMeleeDamagePatch Heated Circuits error: " + ex.ToString());
                    }
                }
                
                // === 3. Cryo Strike Protocol: frostbite + hypothermia ===
                if (attacker.genes != null && attacker.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamedSilentFail("ASE_CryoStrikeProtocol")))
                {
                    try
                    {
                        // Apply frostbite damage
                        HediffDef frostbiteDef = DefDatabase<HediffDef>.GetNamedSilentFail("Frostbite");
                        if (frostbiteDef != null)
                        {
                            BodyPartRecord bodyPart = targetPawn.health.hediffSet.GetNotMissingParts().RandomElement();
                            if (bodyPart != null)
                            {
                                Hediff frostbite = HediffMaker.MakeHediff(frostbiteDef, targetPawn, bodyPart);
                                frostbite.Severity = 0.15f;
                                targetPawn.health.AddHediff(frostbite);
                                
                                Log.Message("[ASE] Cryo-Strike Protocol: " + attacker.LabelShort + " attacked " + targetPawn.LabelShort + 
                                            " -> Applied frostbite (severity: " + frostbite.Severity + ") to " + bodyPart.Label);
                            }
                        }
                        
                        // Apply Hypothermia
                        HediffDef hypothermiaDef = DefDatabase<HediffDef>.GetNamedSilentFail("Hypothermia");
                        if (hypothermiaDef != null)
                        {
                            Hediff existingHypothermia = targetPawn.health.hediffSet.GetFirstHediffOfDef(hypothermiaDef);
                            if (existingHypothermia != null)
                            {
                                existingHypothermia.Severity = System.Math.Min(existingHypothermia.Severity + 0.05f, 1f);
                                Log.Message("[ASE] Cryo-Strike Protocol: " + attacker.LabelShort + " increased hypothermia on " + targetPawn.LabelShort + 
                                            " (severity: " + existingHypothermia.Severity.ToString("F2") + ")");
                            }
                            else
                            {
                                Hediff hypothermia = HediffMaker.MakeHediff(hypothermiaDef, targetPawn);
                                hypothermia.Severity = 0.10f;
                                targetPawn.health.AddHediff(hypothermia);
                                Log.Message("[ASE] Cryo-Strike Protocol: " + attacker.LabelShort + " applied hypothermia to " + targetPawn.LabelShort + 
                                            " (severity: 0.10)");
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error("[ASE] CombinedMeleeDamagePatch Cryo Strike error: " + ex.ToString());
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("[ASE] CombinedMeleeDamagePatch error: " + ex.ToString());
            }
        }
    }
}

