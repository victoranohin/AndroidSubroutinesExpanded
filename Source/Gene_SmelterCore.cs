using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Gene_SmelterCore : Gene
    {
        private float bonusMetalChance = 0.3f; // 30% шанс на бонусные металлы
        private bool wasLastTickSmelting = false; // Отслеживаем предыдущий тик
        private int lastSmeltingJobTick = -1; // Последний тик работы по плавке

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_SmelterCoreHediff"),
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
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_SmelterCoreHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            CheckForSmeltingCompletion();
        }

        private void CheckForSmeltingCompletion()
        {
            // Проверки безопасности
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Map == null)
                return;

            bool isSmeltingNow = (pawn.CurJob != null && (pawn.CurJob.def.defName.Contains("smelt") || pawn.CurJob.def == JobDefOf.DoBill));

            // Если андроид закончил плавку
            if (wasLastTickSmelting && !isSmeltingNow && lastSmeltingJobTick > 0)
            {
                if (Find.TickManager.TicksGame - lastSmeltingJobTick >= 60) // Минимум 1 секунда работы
                {
                    if (Rand.Chance(bonusMetalChance))
                    {
                        GenerateBonusMetals();
                    }
                }
                lastSmeltingJobTick = -1;
            }

            // Если андроид начал плавку
            if (!wasLastTickSmelting && isSmeltingNow)
            {
                lastSmeltingJobTick = Find.TickManager.TicksGame;
            }

            wasLastTickSmelting = isSmeltingNow;
        }

        private void GenerateBonusMetals()
        {
            // Получаем уровень навыка Crafting
            int craftingLevel = pawn.skills.GetSkill(SkillDefOf.Crafting).Level;
            
            // Количество металлов: 1-{craftingLevel}, но не больше 20
            int maxAmount = (craftingLevel < 20) ? craftingLevel : 20;
            if (maxAmount < 1) maxAmount = 1;
            int amount = Rand.Range(1, maxAmount + 1);

            // Определяем доступные металлы на основе уровня навыка
            List<ThingDef> availableMetals = GetAvailableMetalsBySkill(craftingLevel);
            
            if (availableMetals.Count == 0)
                return;

            // Случайно выбираем металл
            ThingDef chosenMetal = availableMetals.RandomElement();

            // Создаем металл
            Thing metal = ThingMaker.MakeThing(chosenMetal, null);
            metal.stackCount = amount;

            // Размещаем рядом с андроидом
            if (!GenPlace.TryPlaceThing(metal, pawn.Position, pawn.Map, ThingPlaceMode.Near))
            {
                metal.Destroy();
            }
            else
            {
                // Уведомление игрока (если включено в настройках)
                bool shouldShowMessages = AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableSmelterCoreMessages;
                if (shouldShowMessages)
                {
                    Messages.Message(pawn.LabelShort + " recovered " + amount + " " + chosenMetal.label + " from advanced smelting!", 
                                   pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        private List<ThingDef> GetAvailableMetalsBySkill(int craftingLevel)
        {
            List<ThingDef> metals = new List<ThingDef>();

            // Уровень 1-5: только сталь
            if (craftingLevel >= 1)
            {
                metals.Add(ThingDefOf.Steel);
            }

            // Уровень 6-10: сталь + серебро
            if (craftingLevel >= 6)
            {
                metals.Add(ThingDefOf.Silver);
            }

            // Уровень 11-15: сталь + серебро + пласталь
            if (craftingLevel >= 11)
            {
                metals.Add(ThingDefOf.Plasteel);
            }

            // Уровень 16-20: сталь + серебро + пласталь + золото
            if (craftingLevel >= 16)
            {
                metals.Add(ThingDefOf.Gold);
            }

            return metals;
        }
    }
}
