using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_ResearchCore_MkIII : Gene
    {
        private int heatGenerationInterval = 60; // Генерируем тепло каждую секунду
        private float heatAmount = 50f; // Количество тепла за раз

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ResearchCoreMkIIIHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 100f;
                    pawn.health.AddHediff(hediff);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ResearchCoreMkIIIHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            // Генерируем тепло каждые 60 тиков (1 секунду)
            if (pawn.IsHashIntervalTick(heatGenerationInterval))
            {
                GenerateHeat();
            }
        }

        private void GenerateHeat()
        {
            // Проверки безопасности
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Map == null)
                return;

            // Генерируем тепло в текущей позиции
            GenTemperature.PushHeat(pawn.Position, pawn.Map, heatAmount);

            // Визуальный эффект жара (иногда)
            if (Rand.Chance(0.1f)) // 10% шанс каждую секунду
            {
                FleckMaker.ThrowHeatGlow(pawn.Position, pawn.Map, 1.5f);
            }

            // Уведомления о перегреве (редко)
            if (pawn.IsHashIntervalTick(2500) && Rand.Chance(0.2f)) // Каждый час с 20% шансом
            {
                float currentTemp = pawn.Position.GetTemperature(pawn.Map);
                if (currentTemp > 35f) // Если температура высокая
                {
                    Messages.Message(pawn.LabelShort + " research core overheating at " + currentTemp.ToString("F0") + "°C", 
                                   pawn, MessageTypeDefOf.NeutralEvent);
                }
            }
        }
    }
}
