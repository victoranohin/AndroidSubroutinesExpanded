using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_Resilience : Hediff
    {
        // Бонусы к броне (хранятся в hediff для применения через statOffsets)
        public float bluntArmorBonus = 0f;
        public float sharpArmorBonus = 0f;

        /// <summary>
        /// Обновляет бонусы к броне из гена
        /// </summary>
        public void UpdateArmorBonuses(float blunt, float sharp)
        {
            bluntArmorBonus = blunt;
            sharpArmorBonus = sharp;
            // Уведомляем об изменении для обновления UI
            if (pawn != null && pawn.health != null)
            {
                pawn.health.Notify_HediffChanged(this);
            }
        }

        /// <summary>
        /// Отображает текущий бонус к броне в скобках (как у Combat Data Logger)
        /// </summary>
        public override string LabelInBrackets
        {
            get
            {
                if (bluntArmorBonus > 0f || sharpArmorBonus > 0f)
                {
                    string result = "";
                    if (bluntArmorBonus > 0f)
                    {
                        result += "+" + (bluntArmorBonus * 100f).ToString("F1") + "% blunt";
                    }
                    if (sharpArmorBonus > 0f)
                    {
                        if (result.Length > 0) result += ", ";
                        result += "+" + (sharpArmorBonus * 100f).ToString("F1") + "% sharp";
                    }
                    return result;
                }
                return "no bonus";
            }
        }

        public override string TipStringExtra
        {
            get
            {
                string baseStr = base.TipStringExtra;
                string effectStr = "";
                
                if (bluntArmorBonus > 0f || sharpArmorBonus > 0f)
                {
                    effectStr = "Blunt armor: +" + (bluntArmorBonus * 100f).ToString("F1") + "%";
                    effectStr += "\nSharp armor: +" + (sharpArmorBonus * 100f).ToString("F1") + "%";
                }
                else
                {
                    effectStr = "No armor bonus yet";
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

        /// <summary>
        /// Переопределяем CurStage для динамического применения statOffsets
        /// </summary>
        public override HediffStage CurStage
        {
            get
            {
                HediffStage stage = new HediffStage();
                stage.minSeverity = 0f;
                stage.label = ""; // Label будет браться из LabelInBrackets
                
                // Добавляем statOffsets для брони
                if (bluntArmorBonus > 0f || sharpArmorBonus > 0f)
                {
                    stage.statOffsets = new List<StatModifier>();
                    
                    if (bluntArmorBonus > 0f)
                    {
                        stage.statOffsets.Add(new StatModifier
                        {
                            stat = StatDefOf.ArmorRating_Blunt,
                            value = bluntArmorBonus
                        });
                    }
                    
                    if (sharpArmorBonus > 0f)
                    {
                        stage.statOffsets.Add(new StatModifier
                        {
                            stat = StatDefOf.ArmorRating_Sharp,
                            value = sharpArmorBonus
                        });
                    }
                }
                
                return stage;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref bluntArmorBonus, "bluntArmorBonus", 0f);
            Scribe_Values.Look(ref sharpArmorBonus, "sharpArmorBonus", 0f);
        }
    }
}

