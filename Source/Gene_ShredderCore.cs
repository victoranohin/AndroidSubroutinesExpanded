using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Gene_ShredderCore : Gene
    {
        private float bonusMaterialChance = 0.5f; // 50% шанс на бонусные материалы
        private bool wasLastTickShredding = false; // Отслеживаем предыдущий тик
        private int lastShreddingJobTick = -1; // Последний тик работы по разборке

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ShredderCoreHediff"),
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
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ShredderCoreHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            CheckForShreddingCompletion();
        }

        private void CheckForShreddingCompletion()
        {
            // Проверки безопасности
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Map == null)
                return;

            bool isShreddingNow = (pawn.CurJob != null && pawn.CurJob.def.defName.Contains("shred"));

            // Если андроид закончил разборку
            if (wasLastTickShredding && !isShreddingNow && lastShreddingJobTick > 0)
            {
                if (Find.TickManager.TicksGame - lastShreddingJobTick >= 60) // Минимум 1 секунда работы
                {
                    if (Rand.Chance(bonusMaterialChance))
                    {
                        GenerateBonusMaterials();
                    }
                }
                lastShreddingJobTick = -1;
            }

            // Если андроид начал разборку
            if (!wasLastTickShredding && isShreddingNow)
            {
                lastShreddingJobTick = Find.TickManager.TicksGame;
            }

            wasLastTickShredding = isShreddingNow;
        }

        private void GenerateBonusMaterials()
        {
            // Получаем уровень навыка Crafting
            int craftingLevel = pawn.skills.GetSkill(SkillDefOf.Crafting).Level;
            
            // Количество компонентов: 1-{craftingLevel}, но не больше 20
            int maxAmount = (craftingLevel < 20) ? craftingLevel : 20;
            if (maxAmount < 1) maxAmount = 1;
            int amount = Rand.Range(1, maxAmount + 1);

            // Определяем доступные материалы на основе уровня навыка
            List<ThingDef> availableMaterials = GetAvailableMaterialsBySkill(craftingLevel);
            
            if (availableMaterials.Count == 0)
                return;

            // Случайно выбираем материал
            ThingDef chosenMaterial = availableMaterials.RandomElement();

            // Создаем материал
            Thing material = ThingMaker.MakeThing(chosenMaterial, null);
            material.stackCount = amount;

            // Размещаем рядом с андроидом
            if (!GenPlace.TryPlaceThing(material, pawn.Position, pawn.Map, ThingPlaceMode.Near))
            {
                material.Destroy();
            }
            else
            {
                // Уведомление игрока (если включено в настройках)
                bool shouldShowMessages = AndroidSubroutinesExpandedMod.Settings == null || AndroidSubroutinesExpandedMod.Settings.enableShredderCoreMessages;
                if (shouldShowMessages)
                {
                    Messages.Message(pawn.LabelShort + " recovered " + amount + " " + chosenMaterial.label + " from mechanoid shredding!", 
                                   pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        private List<ThingDef> GetAvailableMaterialsBySkill(int craftingLevel)
        {
            List<ThingDef> materials = new List<ThingDef>();

            // Уровень 1-5: только сталь
            if (craftingLevel >= 1)
            {
                materials.Add(ThingDefOf.Steel);
            }

            // Уровень 6-10: сталь + компоненты
            if (craftingLevel >= 6)
            {
                materials.Add(ThingDefOf.ComponentIndustrial);
            }

            // Уровень 11-15: сталь + компоненты + пласталь
            if (craftingLevel >= 11)
            {
                materials.Add(ThingDefOf.Plasteel);
            }

            // Уровень 16-20: сталь + компоненты + пласталь + продвинутые компоненты
            if (craftingLevel >= 16)
            {
                materials.Add(ThingDefOf.ComponentSpacer);
            }

            return materials;
        }
    }
}
