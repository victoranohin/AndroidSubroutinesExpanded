using RimWorld;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Базовый класс для всех Anti-Insectoid Plates генов
    /// </summary>
    public abstract class Gene_AntiInsectoidPlates_Base : Gene
    {
        /// <summary>
        /// Возвращает имя hediff для этого уровня пластин
        /// </summary>
        protected abstract string HediffDefName { get; }
        
        /// <summary>
        /// Все возможные имена hediff пластин (для удаления старых при смене уровня)
        /// </summary>
        private static readonly string[] AllPlatesHediffNames = new string[]
        {
            "ASE_AntiInsectoidPlates_MkI",
            "ASE_AntiInsectoidPlates_MkII",
            "ASE_AntiInsectoidPlates_MkIII"
        };
        
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.IsAndroid())
            {
                // Сначала удаляем все старые пластины (на случай смены уровня)
                RemoveAllPlates();
                // Затем добавляем новые
                AddPlates();
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.IsAndroid())
            {
                RemovePlates();
            }
        }

        private void AddPlates()
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null) return;

            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(HediffDefName);
            if (hediffDef != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                Hediff_AntiInsectoidPlates plates = hediff as Hediff_AntiInsectoidPlates;
                if (plates != null)
                {
                    pawn.health.AddHediff(hediff);
                    plates.InitializeArmor();
                }
            }
        }

        private void RemovePlates()
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null) return;

            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(HediffDefName);
            if (hediffDef != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
        
        private void RemoveAllPlates()
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null) return;

            foreach (string hediffName in AllPlatesHediffNames)
            {
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffName);
                if (hediffDef != null)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Anti-Insectoid Plates Mk.I - 80 HP
    /// </summary>
    public class Gene_AntiInsectoidPlates_MkI : Gene_AntiInsectoidPlates_Base
    {
        protected override string HediffDefName { get { return "ASE_AntiInsectoidPlates_MkI"; } }
    }
    
    /// <summary>
    /// Double Anti-Insectoid Plates Mk.II - 160 HP
    /// </summary>
    public class Gene_AntiInsectoidPlates_MkII : Gene_AntiInsectoidPlates_Base
    {
        protected override string HediffDefName { get { return "ASE_AntiInsectoidPlates_MkII"; } }
    }
    
    /// <summary>
    /// Quadro Anti-Insectoid Plates Mk.III - 300 HP
    /// </summary>
    public class Gene_AntiInsectoidPlates_MkIII : Gene_AntiInsectoidPlates_Base
    {
        protected override string HediffDefName { get { return "ASE_AntiInsectoidPlates_MkIII"; } }
    }
    
    /// <summary>
    /// Оставляем старый класс для обратной совместимости (теперь = MkI)
    /// </summary>
    public class Gene_AntiInsectoidPlates_Standard : Gene_AntiInsectoidPlates_MkI
    {
    }
}
