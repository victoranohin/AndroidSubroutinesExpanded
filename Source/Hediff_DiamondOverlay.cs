using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_DiamondOverlay : Hediff
    {
        // Накопленная стоимость (добавляется каждые 1000 тиков)
        // Публичное поле для доступа из патча
        public float accumulatedValue = 0f;
        private int tickCounter = 0;
        private const int TICKS_PER_ACCUMULATION = 1000;
        private const float VALUE_PER_ACCUMULATION = 25f;

        public override void PostTick()
        {
            base.PostTick();
            
            if (pawn == null || pawn.Dead)
                return;
            
            tickCounter++;
            
            // Каждые 1000 тиков добавляем 25 к накопленной стоимости
            if (tickCounter >= TICKS_PER_ACCUMULATION)
            {
                tickCounter = 0;
                accumulatedValue += VALUE_PER_ACCUMULATION;
                
                // Логирование (опционально)
                if (pawn.IsHashIntervalTick(2500))
                {
                    Log.Message("[ASE] Diamond Overlay: " + pawn.LabelShort + 
                               " accumulated value: +" + accumulatedValue.ToString("F0") + " silver");
                }
            }
        }

        public override string TipStringExtra
        {
            get
            {
                string baseStr = base.TipStringExtra;
                string effectStr = "Accumulated value: +" + accumulatedValue.ToString("F0") + " silver";
                effectStr += "\n(+25 silver every 1000 ticks)";
                
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
        /// Переопределяем CurStage для добавления накопленной стоимости к MarketValue
        /// </summary>
        public override HediffStage CurStage
        {
            get
            {
                HediffStage stage = new HediffStage();
                stage.minSeverity = 0f;
                stage.label = "active";
                
                // Добавляем накопленную стоимость к MarketValue через statOffsets
                if (accumulatedValue > 0f)
                {
                    stage.statOffsets = new List<StatModifier>();
                    stage.statOffsets.Add(new StatModifier
                    {
                        stat = StatDefOf.MarketValue,
                        value = accumulatedValue
                    });
                }
                
                return stage;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref accumulatedValue, "accumulatedValue", 0f);
            Scribe_Values.Look(ref tickCounter, "tickCounter", 0);
        }
    }
}

