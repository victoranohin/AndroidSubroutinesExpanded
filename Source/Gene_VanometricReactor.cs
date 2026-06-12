using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_VanometricReactor : Gene
    {
        // Один игровой день = 60000 тиков (2500 тиков/час * 24 часа).
        private const int TicksPerDay = 60000;
        private const int WastePerBatch = 1; // Отходов за одну эмиссию

        // Интервал между эмиссиями вычисляется из настройки "waste packs per day":
        // при N пакетов/день эмитим 1 пакет каждые 60000 / N тиков.
        private int WasteGenerationInterval()
        {
            int perDay = (AndroidSubroutinesExpandedMod.Settings != null)
                ? AndroidSubroutinesExpandedMod.Settings.wastePacksPerDay
                : 7;
            if (perDay < 1) perDay = 1;
            if (perDay > 50) perDay = 50;
            int interval = TicksPerDay / perDay;
            return (interval < 1) ? 1 : interval;
        }

        private bool ShouldLogGeneral()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableGeneralLogging;
        }
        
        private bool ShouldLogVanometric()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableVanometricReactorLogging;
        }
        
        private bool ShouldShowVanometricMessages()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableVanometricReactorMessages;
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_VanometricReactorHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 100f;
                    pawn.health.AddHediff(hediff);
                    if (ShouldLogGeneral())
                    {
                        Log.Message("[ASE] Vanometric Reactor: hediff added to " + pawn.LabelShort);
                    }
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_VanometricReactorHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    if (ShouldLogGeneral())
                    {
                        Log.Message("[ASE] Vanometric Reactor: hediff removed from " + pawn.LabelShort);
                    }
                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(WasteGenerationInterval()))
            {
                GenerateWasteBatch();
            }
        }

        private void GenerateWasteBatch()
        {
            // Проверки безопасности
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Map == null)
                return;

            // Генерируем пачку отходов
            Thing waste = ThingMaker.MakeThing(ThingDefOf.Wastepack, null);
            waste.stackCount = WastePerBatch;
            
            // Размещаем отходы рядом с андроидом
            if (!GenPlace.TryPlaceThing(waste, pawn.Position, pawn.Map, ThingPlaceMode.Near))
            {
                waste.Destroy();
                if (ShouldLogVanometric())
                {
                    Log.Warning("[ASE] Vanometric Reactor: Failed to place waste near " + pawn.LabelShort);
                }
            }
            else
            {
                if (ShouldLogVanometric())
                {
                    Log.Message("[ASE] Vanometric Reactor: Generated " + WastePerBatch + " waste for " + pawn.LabelShort);
                }
                // Игровое уведомление - только если включено в настройках
                if (ShouldShowVanometricMessages())
                {
                    Messages.Message(
                        pawn.LabelShort + "'s vanometric reactor produced " + WastePerBatch + " waste",
                        pawn, 
                        MessageTypeDefOf.NeutralEvent);
                }
            }
        }
    }
}
