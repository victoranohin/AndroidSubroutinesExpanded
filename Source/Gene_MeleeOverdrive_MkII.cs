using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_MeleeOverdrive_MkII : Gene
    {
        private int tickCounter = 0;
        private const int CheckInterval = 250; // Check every 250 ticks

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_MeleeOverdrive_MkIIHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 1f;
                    pawn.health.AddHediff(hediff);
                    Log.Message("[ASE] Melee Overdrive T2 activated on " + pawn.LabelShort + " -> Melee damage +150%, Armor penetration +100%, Cannot use ranged weapons");
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_MeleeOverdrive_MkIIHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            tickCounter++;
            if (tickCounter >= CheckInterval)
            {
                tickCounter = 0;
                CheckAndDropRangedWeapon();
            }
        }

        private void CheckAndDropRangedWeapon()
        {
            if (pawn == null || pawn.equipment == null || pawn.equipment.Primary == null)
            {
                return;
            }

            ThingWithComps weapon = pawn.equipment.Primary;
            if (weapon.def.IsRangedWeapon)
            {
                ThingWithComps droppedWeapon;
                pawn.equipment.TryDropEquipment(weapon, out droppedWeapon, pawn.Position, true);
                Log.Message("[ASE] Melee Overdrive T2: " + pawn.LabelShort + " dropped ranged weapon: " + weapon.Label);
                Messages.Message("ASE_RangedWeaponDropped".Translate(pawn.Name.ToStringShort), 
                    pawn, MessageTypeDefOf.NegativeEvent);
            }
        }
    }
}

