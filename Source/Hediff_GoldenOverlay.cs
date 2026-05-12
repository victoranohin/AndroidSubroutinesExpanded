using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_GoldenOverlay : Hediff
    {
        /// <summary>
        /// Переопределяем CurStage для добавления +20 000 к MarketValue
        /// </summary>
        public override HediffStage CurStage
        {
            get
            {
                HediffStage stage = new HediffStage();
                stage.minSeverity = 0f;
                stage.label = "active";
                
                // Добавляем +20 000 к MarketValue через statOffsets
                stage.statOffsets = new List<StatModifier>();
                stage.statOffsets.Add(new StatModifier
                {
                    stat = StatDefOf.MarketValue,
                    value = 20000f // +20 000 серебра
                });
                
                return stage;
            }
        }
    }
}

