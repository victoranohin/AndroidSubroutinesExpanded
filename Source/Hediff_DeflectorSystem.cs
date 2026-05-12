using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_DeflectorSystem : HediffWithComps
    {
        public override bool ShouldRemove
        {
            get { return false; }
        }

        public override string LabelInBrackets
        {
            get
            {
                return Severity.ToString("F0");
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            // Проверяем, что у андроида есть ген Deflector System
            if (pawn != null && pawn.genes != null)
            {
                bool hasDeflectorSystemGene = pawn.genes.HasActiveGene(
                    DefDatabase<GeneDef>.GetNamedSilentFail("ASE_DeflectorSystem"));
                
                if (!hasDeflectorSystemGene)
                {
                    // Если гена нет, удаляем hediff
                    pawn.health.RemoveHediff(this);
                    Log.Message("DEFLECTOR SYSTEM: Removed hediff from " + pawn.LabelShort + " - gene not found");
                }
            }
        }

        public override bool TendableNow(bool ignoreTimer = false)
        {
            return false;
        }
    }
}
