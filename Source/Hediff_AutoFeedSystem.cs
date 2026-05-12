using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_AutoFeedSystem : Hediff
    {
        public override string LabelInBrackets
        {
            get
            {
                int charges = (int)Severity;
                return charges + "/5";
            }
        }

        public override string TipStringExtra
        {
            get
            {
                int charges = (int)Severity;
                string baseStr = base.TipStringExtra;
                string effectStr = "Charges: " + charges + "/5";
                
                if (charges > 0)
                {
                    effectStr += "\nInstant aiming (aiming time = 0)";
                    effectStr += "\nRanged attack speed: x5 (VEF)";
                }
                else
                {
                    effectStr += "\nNo charges - normal aiming time";
                }
                
                if (string.IsNullOrEmpty(baseStr))
                {
                    return effectStr;
                }
                else
                {
                    return baseStr + "\n" + effectStr;
                }
            }
        }
        
        public override bool TryMergeWith(Hediff other)
        {
            return false;
        }
        
        public override HediffStage CurStage
        {
            get
            {
                HediffStage stage = new HediffStage();
                stage.minSeverity = 0f;
                
                int charges = (int)Severity;
                if (charges > 0)
                {
                    stage.label = "active (" + charges + " charges)";
                    stage.statFactors = new List<StatModifier>();
                    stage.statFactors.Add(new StatModifier
                    {
                        stat = StatDefOf.AimingDelayFactor,
                        value = 0f // aiming time = 0
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
                }
                else
                {
                    stage.label = "empty";
                }
                
                return stage;
            }
        }
        
        /// <summary>
        /// Hediff не должен удаляться даже при severity = 0
        /// </summary>
        public override bool ShouldRemove
        {
            get
            {
                return false;
            }
        }
        
        public override void Tick()
        {
            // Не вызываем base.Tick() чтобы предотвратить автоматическое удаление
            // Hediff должен оставаться даже при severity = 0
        }
    }
}

