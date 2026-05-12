using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_ScrapWarrior : Hediff
    {
        private float marketValue = 0f;
        private float damageBonus = 0f;
        private string tier = "expensive";
        
        public void UpdateMarketValue(float value)
        {
            marketValue = value;
            
            // Определяем бонус урона на основе стоимости
            if (value < 250f)
            {
                damageBonus = 0.50f; // +50%
                tier = "<250";
            }
            else if (value < 500f)
            {
                damageBonus = 0.30f; // +30%
                tier = "250-499";
            }
            else if (value < 1000f)
            {
                damageBonus = 0.20f; // +20%
                tier = "500-999";
            }
            else if (value < 2000f)
            {
                damageBonus = 0.10f; // +10%
                tier = "1000-1999";
            }
            else
            {
                damageBonus = 0f; // No bonus
                tier = "2000+";
            }
        }
        
        public override string LabelInBrackets
        {
            get
            {
                if (damageBonus > 0f)
                {
                    return "+" + (damageBonus * 100f).ToString("F0") + "% damage";
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
                
                effectStr = "Market value: " + marketValue.ToString("F0") + " silver";
                effectStr += "\nValue tier: " + tier;
                if (damageBonus > 0f)
                {
                    effectStr += "\nMelee damage: +" + (damageBonus * 100f).ToString("F0") + "%";
                }
                else
                {
                    effectStr += "\nNo bonus (too expensive)";
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
                
                if (damageBonus > 0f)
                {
                    stage.label = "scrap fury (" + tier + ")";
                    stage.statOffsets = new List<StatModifier>();
                    stage.statOffsets.Add(new StatModifier
                    {
                        stat = StatDefOf.MeleeDamageFactor,
                        value = damageBonus
                    });
                }
                else
                {
                    stage.label = "dormant";
                }
                
                return stage;
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref marketValue, "marketValue", 0f);
            Scribe_Values.Look(ref damageBonus, "damageBonus", 0f);
            Scribe_Values.Look(ref tier, "tier", "expensive");
        }
    }
}

