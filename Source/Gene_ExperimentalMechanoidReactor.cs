using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_ExperimentalMechanoidReactor : Gene
    {
        // Интервал от 3 до 7 дней (1 день = 60000 тиков)
        private const int MinIntervalTicks = 180000;  // 3 дня
        private const int MaxIntervalTicks = 420000;  // 7 дней
        
        // Тик следующего события (сохраняется в save)
        private int nextEventTick = -1;

        private bool ShouldLogGeneral()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableGeneralLogging;
        }
        
        private bool ShouldLogMechanoidReactor()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableVoidReactorLogging;
        }
        
        private bool ShouldShowMechanoidReactorMessages()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableVoidReactorMessages;
        }
        
        /// <summary>
        /// Генерирует случайный интервал от 3 до 7 дней
        /// </summary>
        private void ScheduleNextEvent()
        {
            int randomInterval = Rand.Range(MinIntervalTicks, MaxIntervalTicks);
            nextEventTick = Find.TickManager.TicksGame + randomInterval;
            
            if (ShouldLogMechanoidReactor())
            {
                float daysUntilEvent = (float)randomInterval / 60000f;
                Log.Message("[ASE] Mechanoid Reactor: Next mechanoid attraction scheduled in " + daysUntilEvent.ToString("F1") + " days for " + pawn.LabelShort);
            }
        }
        
        /// <summary>
        /// Сохранение/загрузка данных гена
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextEventTick, "nextEventTick", -1);
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ExperimentalMechanoidReactorHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 100f;
                    pawn.health.AddHediff(hediff);
                    if (ShouldLogGeneral())
                    {
                        Log.Message("[ASE] Experimental Mechanoid Reactor: hediff added to " + pawn.LabelShort);
                    }
                }
                
                // Планируем первое событие
                ScheduleNextEvent();
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ExperimentalMechanoidReactorHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    if (ShouldLogGeneral())
                    {
                        Log.Message("[ASE] Experimental Mechanoid Reactor: hediff removed from " + pawn.LabelShort);
                    }
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            // Проверяем только каждые 250 тиков для производительности
            if (!pawn.IsHashIntervalTick(250))
                return;

            // Инициализация после загрузки сохранения
            if (nextEventTick < 0)
            {
                ScheduleNextEvent();
                return;
            }
            
            // Проверяем, пора ли запускать событие
            if (Find.TickManager.TicksGame >= nextEventTick)
            {
                TriggerMechanoidAttraction();
                ScheduleNextEvent();
            }
        }

        private void TriggerMechanoidAttraction()
        {
            // Проверки безопасности
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Map == null)
                return;

            // Получаем фракцию механоидов
            Faction mechFaction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Mechanoid);
            if (mechFaction == null)
            {
                if (ShouldLogMechanoidReactor())
                {
                    Log.Warning("[ASE] Mechanoid Reactor: Mechanoid faction not found");
                }
                return;
            }

            // Используем инцидент RaidEnemy с фракцией механоидов
            IncidentDef incident = IncidentDefOf.RaidEnemy;
            if (incident == null)
            {
                if (ShouldLogMechanoidReactor())
                {
                    Log.Warning("[ASE] Mechanoid Reactor: RaidEnemy incident not found");
                }
                return;
            }

            // Создаём параметры инцидента
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(incident.category, pawn.Map);
            parms.target = pawn.Map;
            parms.faction = mechFaction;
            parms.forced = true;
            
            // Рассчитываем очки рейда - минимум 100, чтобы избежать ошибки "No raid strategy found"
            float basePoints = StorytellerUtility.DefaultThreatPointsNow(pawn.Map) * 0.3f;
            parms.points = System.Math.Max(100f, basePoints);

            // Выполняем инцидент
            if (incident.Worker.TryExecute(parms))
            {
                if (ShouldLogMechanoidReactor())
                {
                    Log.Message("[ASE] Mechanoid Reactor: Mechanoid raid triggered for " + pawn.LabelShort + " (points: " + parms.points + ")");
                }
                if (ShouldShowMechanoidReactorMessages())
                {
                    Messages.Message(
                        pawn.LabelShort + "'s mechanoid reactor frequency attracted mechanoids!", 
                        pawn, 
                        MessageTypeDefOf.ThreatBig);
                }
            }
            else
            {
                if (ShouldLogMechanoidReactor())
                {
                    Log.Message("[ASE] Mechanoid Reactor: Mechanoid raid failed to trigger (possibly no valid spawn points)");
                }
            }
        }
    }
}

