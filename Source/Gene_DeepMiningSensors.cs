using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Gene_DeepMiningSensors : Gene
    {
        private float additionalResourceChance = 0.1f; // 10% шанс на дополнительные ресурсы
        private bool wasLastTickMining = false; // Отслеживаем предыдущий тик
        private int lastMiningJobTick = -1; // Последний тик работы по добыче

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DeepMiningSensorsHediff"),
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
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DeepMiningSensorsHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            CheckForMiningCompletion();
        }

        private void CheckForMiningCompletion()
        {
            // Проверки безопасности
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Map == null)
                return;

            bool isMiningNow = (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Mine);

            // Если андроид закончил добычу (был на добыче, сейчас не на добыче)
            if (wasLastTickMining && !isMiningNow && lastMiningJobTick > 0)
            {
                // Проверяем, что прошло достаточно тиков для завершения цикла
                if (Find.TickManager.TicksGame - lastMiningJobTick >= 60) // Минимум 1 секунда работы
                {
                    if (Rand.Chance(additionalResourceChance))
                    {
                        GenerateAdditionalResources();
                    }
                }
                lastMiningJobTick = -1;
            }

            // Если андроид начал добычу
            if (!wasLastTickMining && isMiningNow)
            {
                lastMiningJobTick = Find.TickManager.TicksGame;
            }

            wasLastTickMining = isMiningNow;
        }

        private void GenerateAdditionalResources()
        {
            // Получаем уровень навыка Mining
            int miningLevel = pawn.skills.GetSkill(SkillDefOf.Mining).Level;
            
            // Количество ресурсов: 1-{miningLevel}, но не больше 20
            int maxAmount = (miningLevel < 20) ? miningLevel : 20;
            if (maxAmount < 1) maxAmount = 1;
            int amount = Rand.Range(1, maxAmount + 1);

            // Определяем доступные ресурсы на основе уровня навыка
            List<ThingDef> availableResources = GetAvailableResourcesBySkill(miningLevel);
            
            if (availableResources.Count == 0)
                return;

            // Случайно выбираем ресурс
            ThingDef chosenResource = availableResources.RandomElement();

            // Создаем ресурс
            Thing resource = ThingMaker.MakeThing(chosenResource, null);
            resource.stackCount = amount;

            // Размещаем рядом с андроидом
            if (!GenPlace.TryPlaceThing(resource, pawn.Position, pawn.Map, ThingPlaceMode.Near))
            {
                resource.Destroy();
            }
            else
            {
                // Уведомление игрока (если включено в настройках)
                bool shouldShowMessages = AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableDeepMiningSensorsMessages;
                if (shouldShowMessages)
                {
                    Messages.Message(pawn.LabelShort + " sensors detected " + amount + " " + chosenResource.label + " in the rock!", 
                                   pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        private List<ThingDef> GetAvailableResourcesBySkill(int miningLevel)
        {
            List<ThingDef> resources = new List<ThingDef>();

            // Уровень 1-5: только сталь
            if (miningLevel >= 1)
            {
                resources.Add(ThingDefOf.Steel);
            }

            // Уровень 6-10: сталь + серебро
            if (miningLevel >= 6)
            {
                resources.Add(ThingDefOf.Silver);
            }

            // Уровень 11-15: сталь + серебро + пласталь
            if (miningLevel >= 11)
            {
                resources.Add(ThingDefOf.Plasteel);
            }

            // Уровень 16-20: сталь + серебро + пласталь + золото
            if (miningLevel >= 16)
            {
                resources.Add(ThingDefOf.Gold);
            }

            return resources;
        }
    }
}
