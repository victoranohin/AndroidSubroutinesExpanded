using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_BarrageProtocol : Hediff
    {
        public override HediffStage CurStage
        {
            get
            {
                HediffStage stage = new HediffStage();
                stage.minSeverity = 0f;
                stage.label = "active";
                stage.statFactors = new List<StatModifier>();
                
                // Aiming time = 0
                stage.statFactors.Add(new StatModifier
                {
                    stat = StatDefOf.AimingDelayFactor,
                    value = 0f
                });
                
                // Ускорение ranged атаки (VEF_RangeAttackSpeedFactor) - x5 скорость
                // НЕ используем VEF_VerbCooldownFactor - он влияет и на melee cooldown!
                StatDef rangeSpeedStat = DefDatabase<StatDef>.GetNamedSilentFail("VEF_RangeAttackSpeedFactor");
                if (rangeSpeedStat != null)
                {
                    stage.statFactors.Add(new StatModifier
                    {
                        stat = rangeSpeedStat,
                        value = 5.0f // x5 ranged attack speed
                    });
                }
                
                return stage;
            }
        }
    }
}

