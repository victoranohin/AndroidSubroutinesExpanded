using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_ReactiveArmor : HediffWithComps
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
            
            // Проверяем, что у андроида есть ген Reactive Armor
            if (pawn != null && pawn.genes != null)
            {
                bool hasReactiveArmorGene = pawn.genes.HasActiveGene(
                    DefDatabase<GeneDef>.GetNamedSilentFail("ASE_ReactiveArmor"));
                
                if (!hasReactiveArmorGene)
                {
                    // Если гена нет, удаляем hediff
                    pawn.health.RemoveHediff(this);
                    Log.Message("REACTIVE ARMOR: Removed hediff from " + pawn.LabelShort + " - gene not found");
                }
            }
        }

        public override bool TendableNow(bool ignoreTimer = false)
        {
            return false;
        }
    }
}
