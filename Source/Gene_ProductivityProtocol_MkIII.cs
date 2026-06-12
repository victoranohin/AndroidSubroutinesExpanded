using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Gene_ProductivityProtocol_MkIII : Gene
    {
        private HediffDef temperatureHediffDef;
        
        // Лениво резолвим деф: PostAdd не вызывается при загрузке сохранения,
        // поэтому instance-поле нельзя заполнять только там — иначе после загрузки
        // оно остаётся null и адаптация к температуре молча перестаёт работать.
        private void EnsureTemperatureHediffDef()
        {
            if (temperatureHediffDef == null)
            {
                temperatureHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_TemperatureWorkSpeed");
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            EnsureTemperatureHediffDef();
            if (temperatureHediffDef != null)
            {
                CreateTemperatureWorkSpeedHediff();
            }
            
            // Добавляем визуальный hediff
            if (pawn != null && pawn.health != null)
            {
                Hediff visualHediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ProductivityProtocolMkIIIHediff"),
                    pawn);
                if (visualHediff != null)
                {
                    visualHediff.Severity = 100f;
                    pawn.health.AddHediff(visualHediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            // Обновляем скорость работы каждые 60 тиков (примерно каждую секунду)
            if (pawn.IsHashIntervalTick(60))
            {
                UpdateWorkSpeedBasedOnTemperature();
            }
        }

        private void CreateTemperatureWorkSpeedHediff()
        {
            EnsureTemperatureHediffDef();
            if (pawn == null || pawn.health == null || temperatureHediffDef == null)
                return;

            Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(temperatureHediffDef);
            if (existingHediff == null)
            {
                Hediff hediff = HediffMaker.MakeHediff(temperatureHediffDef, pawn);
                hediff.Severity = 1.0f;
                pawn.health.AddHediff(hediff);
            }
        }

        private void UpdateWorkSpeedBasedOnTemperature()
        {
            EnsureTemperatureHediffDef();
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Map == null || temperatureHediffDef == null)
                return;

            float currentTemp = pawn.Position.GetTemperature(pawn.Map);
            
            // Получаем кастомный hediff
            Hediff_TemperatureWorkSpeed hediff = pawn.health.hediffSet.GetFirstHediffOfDef(temperatureHediffDef) as Hediff_TemperatureWorkSpeed;
            if (hediff == null)
            {
                CreateTemperatureWorkSpeedHediff();
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(temperatureHediffDef) as Hediff_TemperatureWorkSpeed;
            }

            if (hediff != null)
            {
                // Простая формула: +4% за каждый градус ниже +20°C, -4% за каждый градус выше +20°C
                // При +20°C — нулевой эффект, при 0°C — +80%, при -20°C — +160%
                float baselineTemp = 20f;
                float tempDifference = baselineTemp - currentTemp; // Разница от +20°C
                float workSpeedOffset = tempDifference * 0.04f; // 4% на градус
                
                // Ограничиваем эффект в разумных пределах (-100% до +200%)
                if (workSpeedOffset < -1.0f) workSpeedOffset = -1.0f;
                if (workSpeedOffset > 2.0f) workSpeedOffset = 2.0f;
                
                // Применяем эффект напрямую к кастомному hediff
                hediff.SetWorkSpeedOffset(workSpeedOffset);
                
                // Устанавливаем severity для отображения
                hediff.Severity = 1.0f + workSpeedOffset;
                
                // Логирование в консоль (если включено)
                bool shouldLog = AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableProductivityProtocolLogging;
                if (shouldLog && pawn.IsHashIntervalTick(2500)) // Каждый час
                {
                    string effectText = (workSpeedOffset >= 0f) ? ("+" + (workSpeedOffset * 100f).ToString("F0") + "%") : ((workSpeedOffset * 100f).ToString("F0") + "%");
                    Log.Message("[ASE] Productivity Protocol Mk.III: " + pawn.LabelShort + " temp=" + currentTemp.ToString("F1") + "°C, effect=" + effectText);
                }
                
                // Игровые уведомления (если включены)
                bool shouldShowMessages = AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableProductivityProtocolMessages;
                if (shouldShowMessages && pawn.IsHashIntervalTick(2500)) // Каждый час
                {
                    if (Rand.Chance(0.3f)) // 30% шанс сообщения каждый час
                    {
                        string effectText = (workSpeedOffset >= 0f) ? ("+" + (workSpeedOffset * 100f).ToString("F0") + "%") : ((workSpeedOffset * 100f).ToString("F0") + "%");
                        Messages.Message(pawn.LabelShort + " temperature adaptation: " + effectText + " work speed at " + currentTemp.ToString("F0") + "°C", 
                                       pawn, MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
        }
        
        // Для применения статов используем hediff подход

        // Очистка при удалении гена
        public override void PostRemove()
        {
            base.PostRemove();
            EnsureTemperatureHediffDef();
            if (pawn != null && pawn.health != null && temperatureHediffDef != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(temperatureHediffDef);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
            
            // Удаляем визуальный hediff
            if (pawn != null && pawn.health != null)
            {
                Hediff visualHediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ProductivityProtocolMkIIIHediff"));
                if (visualHediff != null)
                {
                    pawn.health.RemoveHediff(visualHediff);
                }
            }
        }
    }
}
