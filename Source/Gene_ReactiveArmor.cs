using RimWorld;
using System;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    public class Gene_ReactiveArmor : Gene
    {
        private bool hasExploded = false;

        public override void PostAdd()
        {
            base.PostAdd();
            
            if (pawn != null && pawn.health != null)
            {
                // Удаляем существующий hediff если он есть (от других генов ReactiveArmor)
                HediffWithComps existingReactiveArmor = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ReactiveArmorHediff")) as HediffWithComps;
                
                if (existingReactiveArmor != null)
                {
                    pawn.health.RemoveHediff(existingReactiveArmor);
                    Log.Message("REACTIVE ARMOR: Removed existing hediff from " + pawn.LabelShort + " before adding new one");
                }
                
                // Создаем новый hediff
                HediffWithComps newReactiveArmor = (HediffWithComps)HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ReactiveArmorHediff"), pawn);
                
                // Устанавливаем начальную броню
                newReactiveArmor.Severity = 50f;
                Log.Message("REACTIVE ARMOR: Created hediff for " + pawn.LabelShort + " with 50 armor");
                
                pawn.health.AddHediff(newReactiveArmor);
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            
            if (pawn != null && pawn.health != null)
            {
                // Удаляем hediff при удалении гена
                HediffWithComps reactiveArmor = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ReactiveArmorHediff")) as HediffWithComps;
                
                if (reactiveArmor != null)
                {
                    pawn.health.RemoveHediff(reactiveArmor);
                    Log.Message("REACTIVE ARMOR: Removed hediff from " + pawn.LabelShort + " due to gene removal");
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            // Проверяем состояние андроида для взрыва каждые 60 тиков (1 секунда)
            if (Find.TickManager.TicksGame % 60 == 0 && pawn != null)
            {
                if (!hasExploded && pawn.Downed)
                {
                    Log.Message("REACTIVE ARMOR: " + pawn.LabelShort + " is downed, triggering explosion!");
                    hasExploded = true;
                    
                    // Настоящий взрыв как в VRE: Androids
                    var map = pawn.MapHeld;
                    if (map != null)
                    {
                        var radius = 3f; // Радиус взрыва
                        GenExplosion.DoExplosion(pawn.PositionHeld, pawn.MapHeld, radius, DamageDefOf.Bomb, pawn);
                    }
                    
                    // Сообщение игроку
                    if (pawn.Faction == Faction.OfPlayer)
                    {
                        Messages.Message(pawn.LabelShort + " reactive armor exploded when downed!", 
                                       pawn, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
            
            // Убеждаемся что hediff остается видимым каждые 120 тиков (2 секунды)
            if (pawn != null && pawn.health != null && Find.TickManager.TicksGame % 120 == 0)
            {
                HediffWithComps hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ReactiveArmorHediff")) as HediffWithComps;
                
                if (hediff != null && hediff.Severity != 50f)
                {
                    hediff.Severity = 50f;
                    Log.Message("REACTIVE ARMOR: Fixed hediff severity for " + pawn.LabelShort);
                }
            }
        }
    }
}
