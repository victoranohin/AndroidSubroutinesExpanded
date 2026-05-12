using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_TemperatureWorkSpeed : Hediff
    {
        private float currentWorkSpeedOffset = 0f;
        
        public void SetWorkSpeedOffset(float offset)
        {
            currentWorkSpeedOffset = offset;
        }
        
        public override string LabelInBrackets
        {
            get
            {
                if (currentWorkSpeedOffset > 0f)
                {
                    return "+" + (currentWorkSpeedOffset * 100f).ToString("F0") + "%";
                }
                else if (currentWorkSpeedOffset < 0f)
                {
                    return (currentWorkSpeedOffset * 100f).ToString("F0") + "%";
                }
                else
                {
                    return "0%";
                }
            }
        }

        public override string TipStringExtra
        {
            get
            {
                string baseStr = base.TipStringExtra;
                string effectStr = "";
                
                if (currentWorkSpeedOffset != 0f)
                {
                    string sign = (currentWorkSpeedOffset > 0f) ? "+" : "";
                    effectStr = "Work speed: " + sign + (currentWorkSpeedOffset * 100f).ToString("F0") + "%";
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
        
        // Применяем статовые модификаторы динамически
        public override void PostTick()
        {
            base.PostTick();
        }
        
        // Переопределяем для применения статов
        public override bool TryMergeWith(Hediff other)
        {
            return false; // Не объединяем с другими hediff
        }
        
        // Переопределяем CurStage для динамического применения статов
        public override HediffStage CurStage
        {
            get
            {
                HediffStage stage = new HediffStage();
                stage.minSeverity = 0f;
                stage.label = "temperature adaptation";
                
                if (currentWorkSpeedOffset != 0f)
                {
                    stage.statOffsets = new List<StatModifier>();
                    stage.statOffsets.Add(new StatModifier
                    {
                        stat = StatDefOf.WorkSpeedGlobal,
                        value = currentWorkSpeedOffset
                    });
                }
                
                return stage;
            }
        }
    }
}
