using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AndroidSubroutinesExpanded
{
    public class Gene_UnarmoredEvasion : Gene
    {
        private HediffDef evasionHediffDef;
        private bool initialized = false;
        
        public override void PostAdd()
        {
            base.PostAdd();
            InitializeHediff();
            AddEvasionHediff();
        }
        
        private void InitializeHediff()
        {
            if (!initialized)
            {
                evasionHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_UnarmoredEvasionHediff");
                initialized = true;
            }
        }
        
        private void AddEvasionHediff()
        {
            if (pawn == null || pawn.health == null || evasionHediffDef == null)
                return;
                
            Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(evasionHediffDef);
            if (existingHediff == null)
            {
                Hediff hediff = HediffMaker.MakeHediff(evasionHediffDef, pawn);
                hediff.Severity = 1f;
                pawn.health.AddHediff(hediff);
                
                if (ShouldLog())
                {
                    Log.Message("[ASE] Unarmored Evasion: Activated on " + pawn.LabelShort);
                }
            }
        }
        
        public override void Tick()
        {
            base.Tick();
            
            // Обновляем бонус каждые 120 тиков (примерно каждые 2 секунды)
            if (pawn.IsHashIntervalTick(120))
            {
                UpdateEvasionBonus();
            }
        }
        
        private void UpdateEvasionBonus()
        {
            if (pawn == null || pawn.Dead || pawn.health == null)
                return;
                
            InitializeHediff();
            if (evasionHediffDef == null)
                return;
            
            Hediff_UnarmoredEvasion hediff = pawn.health.hediffSet.GetFirstHediffOfDef(evasionHediffDef) as Hediff_UnarmoredEvasion;
            if (hediff == null)
            {
                AddEvasionHediff();
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(evasionHediffDef) as Hediff_UnarmoredEvasion;
            }
            
            if (hediff == null)
                return;
            
            // Подсчитываем пустые слоты одежды
            int emptySlots = CountEmptyApparelSlots();
            
            hediff.UpdateEmptySlots(emptySlots);
            
            // Логирование при изменении
            if (ShouldLog() && pawn.IsHashIntervalTick(2500))
            {
                if (emptySlots > 0)
                {
                    Log.Message("[ASE] Unarmored Evasion: " + pawn.LabelShort + 
                               " has " + emptySlots + " empty slots" +
                               " -> +" + (emptySlots * 10) + " melee dodge");
                }
            }
        }
        
        private int CountEmptyApparelSlots()
        {
            if (pawn == null || pawn.apparel == null)
                return 0;
            
            // Получаем все доступные слоты для пешки
            List<BodyPartGroupDef> availableSlots = new List<BodyPartGroupDef>();
            
            // Основные слоты одежды
            if (BodyPartGroupDefOf.Torso != null) availableSlots.Add(BodyPartGroupDefOf.Torso);
            if (BodyPartGroupDefOf.Legs != null) availableSlots.Add(BodyPartGroupDefOf.Legs);
            if (BodyPartGroupDefOf.FullHead != null) availableSlots.Add(BodyPartGroupDefOf.FullHead);
            if (BodyPartGroupDefOf.UpperHead != null) availableSlots.Add(BodyPartGroupDefOf.UpperHead);
            if (BodyPartGroupDefOf.Eyes != null) availableSlots.Add(BodyPartGroupDefOf.Eyes);
            
            // Подсчитываем сколько слотов занято
            int wornCount = pawn.apparel.WornApparelCount;
            
            // Простой подход: максимум 8 слотов, занято wornCount
            int maxSlots = 8;
            int emptySlots = maxSlots - wornCount;
            
            if (emptySlots < 0) emptySlots = 0;
            if (emptySlots > maxSlots) emptySlots = maxSlots;
            
            return emptySlots;
        }
        
        private bool ShouldLog()
        {
            return AndroidSubroutinesExpandedMod.Settings == null || 
                   AndroidSubroutinesExpandedMod.Settings.enableUnarmoredEvasionLogging;
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                InitializeHediff();
                if (evasionHediffDef != null)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(evasionHediffDef);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
}
