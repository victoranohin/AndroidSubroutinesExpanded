using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_ToxicDischarge : Hediff
    {
        public override string LabelInBrackets
        {
            get
            {
                int charges = (int)Severity;
                return charges + "/5";
            }
        }

        public override string TipStringExtra
        {
            get
            {
                int charges = (int)Severity;
                string baseStr = base.TipStringExtra;
                string effectStr = "Charges: " + charges + "/5";
                effectStr += "\nRecharge: 1 charge per 1000 ticks";
                
                if (charges > 0)
                {
                    effectStr += "\nActive: Creates toxic cloud on damage taken";
                }
                else
                {
                    effectStr += "\nNo charges - system inactive";
                }
                
                if (string.IsNullOrEmpty(baseStr))
                {
                    return effectStr;
                }
                else
                {
                    return baseStr + "\n" + effectStr;
                }
            }
        }
        
        public override bool TryMergeWith(Hediff other)
        {
            return false;
        }
        
        /// <summary>
        /// Hediff не должен удаляться даже при severity = 0
        /// </summary>
        public override bool ShouldRemove
        {
            get
            {
                return false;
            }
        }
        
        public override void Tick()
        {
            // Не вызываем base.Tick() чтобы предотвратить автоматическое удаление
            // Hediff должен оставаться даже при severity = 0
        }
    }
}

