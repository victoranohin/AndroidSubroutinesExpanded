using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_DiplomaticCore : Gene
    {
        private const int CheckInterval = 250;

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DiplomaticCoreHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 100f;
                    pawn.health.AddHediff(hediff);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_DiplomaticCoreHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            if (pawn == null || pawn.Dead)
                return;

            if (pawn.IsHashIntervalTick(CheckInterval))
            {
                CheckAndDropRangedWeapon();
            }
        }

        private void CheckAndDropRangedWeapon()
        {
            if (pawn == null || pawn.equipment == null || pawn.equipment.Primary == null)
                return;

            ThingWithComps weapon = pawn.equipment.Primary;
            if (weapon.def.IsRangedWeapon)
            {
                ThingWithComps droppedWeapon;
                pawn.equipment.TryDropEquipment(weapon, out droppedWeapon, pawn.Position, true);
                Log.Message("[ASE] Diplomatic Core: " + pawn.LabelShort + " dropped ranged weapon: " + weapon.Label);
            }
        }
    }
}
