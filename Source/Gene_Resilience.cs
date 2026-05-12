using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_Resilience : Gene
    {
        // Храним накопленные бонусы к броне (в процентах)
        public float bluntArmorBonus = 0f;
        public float sharpArmorBonus = 0f;

        /// <summary>
        /// Получает hediff Resilience для обновления бонусов
        /// </summary>
        private Hediff_Resilience GetResilienceHediff()
        {
            if (pawn == null || pawn.health == null)
                return null;
            
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ResilienceHediff"));
            
            return hediff as Hediff_Resilience;
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff_Resilience hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ResilienceHediff"),
                    pawn) as Hediff_Resilience;
                if (hediff != null)
                {
                    hediff.Severity = 1f;
                    hediff.UpdateArmorBonuses(bluntArmorBonus, sharpArmorBonus);
                    pawn.health.AddHediff(hediff);
                    Log.Message("[ASE] Resilience gene activated on " + pawn.LabelShort);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ResilienceHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    Log.Message("[ASE] Resilience gene removed from " + pawn.LabelShort);
                }
            }
        }

        /// <summary>
        /// Вызывается при получении урона для увеличения брони
        /// </summary>
        public void OnDamageTaken()
        {
            bluntArmorBonus += 0.001f; // 0.1% = 0.001
            sharpArmorBonus += 0.001f;
            
            // Обновляем hediff с новыми значениями
            Hediff_Resilience hediff = GetResilienceHediff();
            if (hediff != null)
            {
                hediff.UpdateArmorBonuses(bluntArmorBonus, sharpArmorBonus);
            }
            
            Log.Message("[ASE] Resilience: " + pawn.LabelShort + 
                       " armor increased by 0.1% (Blunt: +" + (bluntArmorBonus * 100f).ToString("F1") + 
                       "%, Sharp: +" + (sharpArmorBonus * 100f).ToString("F1") + "%)");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref bluntArmorBonus, "bluntArmorBonus", 0f);
            Scribe_Values.Look(ref sharpArmorBonus, "sharpArmorBonus", 0f);
            
            // После загрузки обновляем hediff
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Hediff_Resilience hediff = GetResilienceHediff();
                if (hediff != null)
                {
                    hediff.UpdateArmorBonuses(bluntArmorBonus, sharpArmorBonus);
                }
            }
        }
    }
}

