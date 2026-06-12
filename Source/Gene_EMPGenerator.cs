using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AndroidSubroutinesExpanded
{
    public class Gene_EMPGenerator : Gene
    {
        private const int EMP_MIN_INTERVAL = 1000;
        private const int EMP_MAX_INTERVAL = 5000;
        private const float EMP_RADIUS = 5f;
        
        private int nextEMPTick = 0;

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_EMPGeneratorHediff"),
                    pawn);
                if (hediff != null)
                {
                    hediff.Severity = 100f;
                    pawn.health.AddHediff(hediff);
                }
            }
            // Установить первый интервал
            SetNextEMPTick();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (pawn != null && pawn.health != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamedSilentFail("ASE_EMPGeneratorHediff"));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
        
        private void SetNextEMPTick()
        {
            nextEMPTick = Find.TickManager.TicksGame + Rand.Range(EMP_MIN_INTERVAL, EMP_MAX_INTERVAL);
        }

        public override void Tick()
        {
            base.Tick();
            
            if (pawn == null || pawn.Dead || pawn.Map == null)
                return;

            if (Find.TickManager.TicksGame >= nextEMPTick)
            {
                EmitEMP();
                SetNextEMPTick();
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextEMPTick, "nextEMPTick", 0);
        }

        private void EmitEMP()
        {
            if (pawn == null || pawn.Map == null)
                return;

            // Исключаем самого носителя из собственного ЭМИ-взрыва, иначе андроид
            // регулярно оглушает сам себя (андроиды VRE уязвимы к EMP).
            List<Thing> ignored = new List<Thing> { pawn };

            GenExplosion.DoExplosion(
                center: pawn.Position,
                map: pawn.Map,
                radius: EMP_RADIUS,
                damType: DamageDefOf.EMP,
                instigator: pawn,
                damAmount: 50,
                armorPenetration: 0f,
                explosionSound: null,
                weapon: null,
                projectile: null,
                intendedTarget: null,
                postExplosionSpawnThingDef: null,
                postExplosionSpawnChance: 0f,
                postExplosionSpawnThingCount: 0,
                postExplosionGasType: null,
                applyDamageToExplosionCellsNeighbors: false,
                preExplosionSpawnThingDef: null,
                preExplosionSpawnChance: 0f,
                preExplosionSpawnThingCount: 0,
                chanceToStartFire: 0f,
                damageFalloff: true,
                direction: null,
                ignoredThings: ignored,
                affectedAngle: null,
                doVisualEffects: true,
                propagationSpeed: 1f,
                excludeRadius: 0f,
                doSoundEffects: false,
                screenShakeFactor: 0f
            );
        }
    }
}
