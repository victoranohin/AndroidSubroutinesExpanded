using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_BerserkerProtocol : Gene
    {
        private HediffDef berserkerHediffDef;
        private bool initialized = false;
        
        public override void PostAdd()
        {
            base.PostAdd();
            InitializeHediff();
            AddBerserkerHediff();
        }
        
        private void InitializeHediff()
        {
            if (!initialized)
            {
                berserkerHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_BerserkerProtocolHediff");
                initialized = true;
            }
        }
        
        private void AddBerserkerHediff()
        {
            if (pawn == null || pawn.health == null || berserkerHediffDef == null)
                return;
                
            Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(berserkerHediffDef);
            if (existingHediff == null)
            {
                Hediff hediff = HediffMaker.MakeHediff(berserkerHediffDef, pawn);
                hediff.Severity = 1f;
                pawn.health.AddHediff(hediff);
                
                if (ShouldLog())
                {
                    Log.Message("[ASE] Berserker Protocol: Activated on " + pawn.LabelShort);
                }
            }
        }
        
        public override void Tick()
        {
            base.Tick();
            
            // Обновляем бонус урона каждые 60 тиков
            if (pawn.IsHashIntervalTick(60))
            {
                UpdateDamageBasedOnHealth();
            }
        }
        
        private void UpdateDamageBasedOnHealth()
        {
            if (pawn == null || pawn.Dead || pawn.health == null)
                return;
                
            InitializeHediff();
            if (berserkerHediffDef == null)
                return;
            
            Hediff_BerserkerProtocol hediff = pawn.health.hediffSet.GetFirstHediffOfDef(berserkerHediffDef) as Hediff_BerserkerProtocol;
            if (hediff == null)
            {
                AddBerserkerHediff();
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(berserkerHediffDef) as Hediff_BerserkerProtocol;
            }
            
            if (hediff == null)
                return;
            
            // Вычисляем процент здоровья
            float healthPercent = pawn.health.summaryHealth.SummaryHealthPercent;
            
            // Определяем бонус урона на основе порогов HP
            float damageBonus = 0f;
            string tier = "normal";
            
            if (healthPercent < 0.15f)
            {
                damageBonus = 1.20f; // +120% при <15% HP
                tier = "<15% HP";
            }
            else if (healthPercent < 0.25f)
            {
                damageBonus = 0.70f; // +70% при <25% HP
                tier = "<25% HP";
            }
            else if (healthPercent < 0.50f)
            {
                damageBonus = 0.40f; // +40% при <50% HP
                tier = "<50% HP";
            }
            else if (healthPercent < 0.80f)
            {
                damageBonus = 0.15f; // +15% при <80% HP
                tier = "<80% HP";
            }
            
            // Применяем бонус к hediff
            hediff.SetDamageBonus(damageBonus, tier);
            
            // Логирование при изменении
            if (ShouldLog() && pawn.IsHashIntervalTick(2500))
            {
                if (damageBonus > 0f)
                {
                    Log.Message("[ASE] Berserker Protocol: " + pawn.LabelShort + 
                               " HP=" + (healthPercent * 100f).ToString("F0") + "%" +
                               " -> +" + (damageBonus * 100f).ToString("F0") + "% melee damage");
                }
            }
            
            // Игровые сообщения при активации берсерка
            if (ShouldShowMessages() && damageBonus > 0f && pawn.IsHashIntervalTick(2500))
            {
                if (Rand.Chance(0.2f))
                {
                    Messages.Message(pawn.LabelShort + " berserker mode: +" + (damageBonus * 100f).ToString("F0") + "% damage at " + (healthPercent * 100f).ToString("F0") + "% HP",
                                   pawn, MessageTypeDefOf.NeutralEvent);
                }
            }
        }
        
        private bool ShouldLog()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || 
                   AndroidSubroutinesExpandedMod.Settings.enableBerserkerProtocolLogging;
        }
        
        private bool ShouldShowMessages()
        {
            return AndroidSubroutinesExpandedMod.Settings != null && 
                   AndroidSubroutinesExpandedMod.Settings.enableBerserkerProtocolLogging;
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                InitializeHediff();
                if (berserkerHediffDef != null)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(berserkerHediffDef);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
}
