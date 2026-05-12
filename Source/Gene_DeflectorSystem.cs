using RimWorld;
using System;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    public class Gene_DeflectorSystem : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            
            if (pawn != null && pawn.health != null)
            {
                // Удаляем существующий hediff если он есть (от других генов DeflectorSystem)
                HediffWithComps existingDeflectorSystem = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DeflectorSystemHediff")) as HediffWithComps;
                
                if (existingDeflectorSystem != null)
                {
                    pawn.health.RemoveHediff(existingDeflectorSystem);
                    Log.Message("DEFLECTOR SYSTEM: Removed existing hediff from " + pawn.LabelShort + " before adding new one");
                }
                
                // Создаем новый hediff
                HediffWithComps newDeflectorSystem = (HediffWithComps)HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DeflectorSystemHediff"), pawn);
                
                // Устанавливаем начальную броню
                newDeflectorSystem.Severity = 100f;
                Log.Message("DEFLECTOR SYSTEM: Created hediff for " + pawn.LabelShort + " with 100 armor");
                
                pawn.health.AddHediff(newDeflectorSystem);
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            
            if (pawn != null && pawn.health != null)
            {
                // Удаляем hediff при удалении гена
                HediffWithComps hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DeflectorSystemHediff")) as HediffWithComps;
                
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    Log.Message("DEFLECTOR SYSTEM: Removed hediff from " + pawn.LabelShort);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            // Основная логика в Harmony патче
        }

        // Метод для проверки отражения атаки (50% шанс)
        public static bool ShouldReflectAttack()
        {
            return Rand.Value < 0.5f;
        }

        // Метод для отражения урона обратно к атакующему
        public static void ReflectDamageBack(Pawn target, Pawn attacker, float damageAmount, DamageDef damageType)
        {
            if (target == null || attacker == null || damageAmount <= 0f) return;

            // Наносим отраженный урон атакующему
            var reflectedDamage = new DamageInfo(damageType, damageAmount, 0f, -1f, target);
            attacker.TakeDamage(reflectedDamage);
            
            Log.Message("DEFLECTOR SYSTEM: " + target.LabelShort + " reflected " + damageAmount.ToString("F1") + " damage back to " + attacker.LabelShort);
        }
    }
}
