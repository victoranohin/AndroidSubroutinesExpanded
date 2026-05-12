using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_UnarmoredEvasion : Hediff
    {
        private int emptySlots = 0;
        private float meleeDodgeBonus = 0f;
        
        // Максимальное количество слотов одежды (примерно 8-10 в ванилле)
        private const int MAX_APPAREL_SLOTS = 10;
        private const float MELEE_DODGE_PER_SLOT = 15f;  // +15% за слот (абсолютное значение для MeleeDodgeChance)
        private const float RANGED_DODGE_PER_SLOT = 0.15f;  // +15% за слот (доля для VEF_RangedDodgeChance)
        
        private float rangedDodgeBonus = 0f;
        
        public void UpdateEmptySlots(int slots)
        {
            emptySlots = slots;
            meleeDodgeBonus = slots * MELEE_DODGE_PER_SLOT;
            rangedDodgeBonus = slots * RANGED_DODGE_PER_SLOT;
        }
        
        public override string LabelInBrackets
        {
            get
            {
                if (emptySlots > 0)
                {
                    return emptySlots + " empty slots";
                }
                return "fully clothed";
            }
        }

        public override string TipStringExtra
        {
            get
            {
                string baseStr = base.TipStringExtra;
                string effectStr = "";
                
                if (emptySlots > 0)
                {
                    effectStr = "Empty apparel slots: " + emptySlots;
                    effectStr += "\nMelee dodge: +" + meleeDodgeBonus.ToString("F0") + "%";
                    effectStr += "\nRanged dodge (VEF): +" + (rangedDodgeBonus * 100f).ToString("F0") + "%";
                }
                else
                {
                    effectStr = "No bonus - wearing full apparel";
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
                
                if (emptySlots > 0)
                {
                    stage.label = "agile (" + emptySlots + " slots)";
                    stage.statOffsets = new List<StatModifier>();
                    stage.statOffsets.Add(new StatModifier
                    {
                        stat = StatDefOf.MeleeDodgeChance,
                        value = meleeDodgeBonus
                    });
                    // VEF_RangedDodgeChance - добавляем если VEF доступен
                    StatDef rangedDodgeStat = DefDatabase<StatDef>.GetNamedSilentFail("VEF_RangedDodgeChance");
                    if (rangedDodgeStat != null)
                    {
                        stage.statOffsets.Add(new StatModifier
                        {
                            stat = rangedDodgeStat,
                            value = rangedDodgeBonus // доля для VEF stat
                        });
                    }
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
            Scribe_Values.Look(ref emptySlots, "emptySlots", 0);
            Scribe_Values.Look(ref meleeDodgeBonus, "meleeDodgeBonus", 0f);
            Scribe_Values.Look(ref rangedDodgeBonus, "rangedDodgeBonus", 0f);
        }
    }
}

