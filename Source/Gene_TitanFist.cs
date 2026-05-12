using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_TitanFist : Gene
    {
        private static HediffDef titanFistHediffDef;
        
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                if (titanFistHediffDef == null)
                {
                    titanFistHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_TitanFistHediff");
                }
                
                if (titanFistHediffDef != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(titanFistHediffDef, pawn);
                    if (hediff != null)
                    {
                        hediff.Severity = 1f;
                        pawn.health.AddHediff(hediff);
                        
                        if (AndroidSubroutinesExpandedMod.Settings == null || 
                            AndroidSubroutinesExpandedMod.Settings.enableGeneralLogging)
                        {
                            Log.Message("[ASE] Titan Fist: Activated on " + pawn.LabelShort + " -> ×5 melee damage, -50% move speed");
                        }
                    }
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                if (titanFistHediffDef == null)
                {
                    titanFistHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("ASE_TitanFistHediff");
                }
                
                if (titanFistHediffDef != null)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(titanFistHediffDef);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
}

