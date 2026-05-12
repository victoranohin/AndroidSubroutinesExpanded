using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_BerserkerProtocol : Hediff
    {
        private float currentDamageBonus = 0f;
        private string currentTier = "normal";
        
        public void SetDamageBonus(float bonus, string tier)
        {
            currentDamageBonus = bonus;
            currentTier = tier;
        }
        
        public override string LabelInBrackets
        {
            get
            {
                if (currentDamageBonus > 0f)
                {
                    return "+" + (currentDamageBonus * 100f).ToString("F0") + "% damage";
                }
                return "standby";
            }
        }

        public override string TipStringExtra
        {
            get
            {
                string baseStr = base.TipStringExtra;
                string effectStr = "";
                
                if (currentDamageBonus > 0f)
                {
                    effectStr = "Berserker mode: +" + (currentDamageBonus * 100f).ToString("F0") + "% melee damage";
                    effectStr += "\nHealth threshold: " + currentTier;
                }
                else
                {
                    effectStr = "Berserker mode: standby (health > 80%)";
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
                
                if (currentDamageBonus > 0f)
                {
                    stage.label = "berserker rage";
                    stage.statOffsets = new List<StatModifier>();
                    stage.statOffsets.Add(new StatModifier
                    {
                        stat = StatDefOf.MeleeDamageFactor,
                        value = currentDamageBonus
                    });
                }
                else
                {
                    stage.label = "standby";
                }
                
                return stage;
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentDamageBonus, "currentDamageBonus", 0f);
            Scribe_Values.Look(ref currentTier, "currentTier", "normal");
        }
    }
}

