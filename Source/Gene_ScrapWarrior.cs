using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_ScrapWarrior : Gene
    {
        private HediffDef scrapWarriorHediffDef;
        private bool initialized = false;
        
        public override void PostAdd()
        {
            base.PostAdd();
            InitializeHediff();
            AddScrapWarriorHediff();
        }
        
        private void InitializeHediff()
        {
            if (!initialized)
            {
                scrapWarriorHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_ScrapWarriorHediff");
                initialized = true;
            }
        }
        
        private void AddScrapWarriorHediff()
        {
            if (pawn == null || pawn.health == null || scrapWarriorHediffDef == null)
                return;
                
            Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(scrapWarriorHediffDef);
            if (existingHediff == null)
            {
                Hediff hediff = HediffMaker.MakeHediff(scrapWarriorHediffDef, pawn);
                hediff.Severity = 1f;
                pawn.health.AddHediff(hediff);
                
                if (ShouldLog())
                {
                    Log.Message("[ASE] Scrap Warrior: Activated on " + pawn.LabelShort);
                }
            }
        }
        
        public override void Tick()
        {
            base.Tick();
            
            // Обновляем бонус каждые 250 тиков (market value не меняется часто)
            if (pawn.IsHashIntervalTick(250))
            {
                UpdateDamageBonus();
            }
        }
        
        private void UpdateDamageBonus()
        {
            if (pawn == null || pawn.Dead || pawn.health == null)
                return;
                
            InitializeHediff();
            if (scrapWarriorHediffDef == null)
                return;
            
            Hediff_ScrapWarrior hediff = pawn.health.hediffSet.GetFirstHediffOfDef(scrapWarriorHediffDef) as Hediff_ScrapWarrior;
            if (hediff == null)
            {
                AddScrapWarriorHediff();
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(scrapWarriorHediffDef) as Hediff_ScrapWarrior;
            }
            
            if (hediff == null)
                return;
            
            // Получаем рыночную стоимость пешки
            float marketValue = pawn.MarketValue;
            
            hediff.UpdateMarketValue(marketValue);
            
            // Логирование
            if (ShouldLog() && pawn.IsHashIntervalTick(2500))
            {
                string bonus = "";
                if (marketValue < 250f) bonus = "+50%";
                else if (marketValue < 500f) bonus = "+30%";
                else if (marketValue < 1000f) bonus = "+20%";
                else if (marketValue < 2000f) bonus = "+10%";
                else bonus = "0%";
                
                Log.Message("[ASE] Scrap Warrior: " + pawn.LabelShort + 
                           " value=" + marketValue.ToString("F0") +
                           " -> " + bonus + " melee damage");
            }
        }
        
        private bool ShouldLog()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || 
                   AndroidSubroutinesExpandedMod.Settings.enableScrapWarriorLogging;
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                InitializeHediff();
                if (scrapWarriorHediffDef != null)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(scrapWarriorHediffDef);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
}
