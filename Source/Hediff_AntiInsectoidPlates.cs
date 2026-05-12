using RimWorld;
using Verse;
using VREAndroids;

namespace AndroidSubroutinesExpanded
{
    public class Hediff_AntiInsectoidPlates : HediffWithComps
    {
        private static bool ShouldLog()
        {
            return AndroidSubroutinesExpandedMod.Settings != null && 
                   AndroidSubroutinesExpandedMod.Settings.enableAntiInsectoidPlatesLogging;
        }
        
        public override string Label
        {
            get
            {
                float maxArmor = GetMaxArmor();
                return def.label + " (" + Severity.ToString("F0") + "/" + maxArmor.ToString("F0") + ")";
            }
        }

        public void InitializeArmor()
        {
            if (pawn != null && pawn.IsAndroid())
            {
                float maxArmor = GetMaxArmor();
                Severity = maxArmor;
                
                if (ShouldLog())
                {
                    Log.Message("[ASE PLATES] InitializeArmor for " + pawn.LabelShort + " | " + def.defName + " | Set to: " + maxArmor.ToString("F1"));
                }
            }
        }

        /// <summary>
        /// Возвращает максимальную прочность из определения hediff
        /// </summary>
        public float GetMaxArmor()
        {
            return def.maxSeverity;
        }
        
        // Оставляем для обратной совместимости
        public float GetMaxArmorForGenes()
        {
            return GetMaxArmor();
        }

        public void RepairArmor(float amount)
        {
            float maxArmor = GetMaxArmor();
            if (maxArmor > 0f)
            {
                float oldSeverity = Severity;
                
                // Если Severity отрицательный (баг), сначала сбрасываем до 0
                if (Severity < 0f)
                {
                    Log.Warning("[ASE PLATES] Negative severity detected (" + Severity.ToString("F1") + "), resetting to 0");
                    Severity = 0f;
                }
                
                Severity = System.Math.Min(Severity + amount, maxArmor);
                
                if (ShouldLog())
                {
                    Log.Message("[ASE PLATES] RepairArmor called | Amount: " + amount.ToString("F1") + 
                               " | Before: " + oldSeverity.ToString("F1") + 
                               " | After: " + Severity.ToString("F1") + 
                               " | Max: " + maxArmor.ToString("F1"));
                }
            }
        }
        
        /// <summary>
        /// Полностью восстанавливает броню до максимума
        /// </summary>
        public void FullRepair()
        {
            float maxArmor = GetMaxArmor();
            if (maxArmor > 0f)
            {
                float oldSeverity = Severity;
                Severity = maxArmor;
                
                if (ShouldLog())
                {
                    Log.Message("[ASE PLATES] FullRepair | Before: " + oldSeverity.ToString("F1") + " | After: " + Severity.ToString("F1"));
                }
            }
        }
        
        /// <summary>
        /// Вызывается при получении урона (из CombinedArmorPatch)
        /// </summary>
        public void TakeDamage(float damage)
        {
            float oldSeverity = Severity;
            Severity -= damage;
            
            // Защита от отрицательных значений
            if (Severity < 0f)
            {
                if (ShouldLog())
                {
                    Log.Warning("[ASE PLATES] TakeDamage resulted in negative (" + Severity.ToString("F1") + "), clamping to 0");
                }
                Severity = 0f;
            }
            
            if (ShouldLog())
            {
                Log.Message("[ASE PLATES] TakeDamage | Damage: " + damage.ToString("F1") + 
                           " | Before: " + oldSeverity.ToString("F1") + 
                           " | After: " + Severity.ToString("F1"));
            }
        }

        public override void Tick()
        {
            // Не вызываем base.Tick() чтобы предотвратить автоматическое удаление
            // Hediff должен оставаться даже при severity = 0
            
            if (pawn == null) return;
            
            // Проверяем каждые 250 тиков
            if (!pawn.IsHashIntervalTick(250)) return;
            
            // Защита от отрицательных значений
            if (Severity < 0f)
            {
                Log.Warning("[ASE PLATES] Negative severity in Tick (" + Severity.ToString("F1") + "), resetting to 0");
                Severity = 0f;
            }
        }
        
        /// <summary>
        /// Проверяет, нужен ли ремонт пластин
        /// </summary>
        public bool NeedsRepair()
        {
            float maxArmor = GetMaxArmor();
            return maxArmor > 0f && Severity < maxArmor;
        }
        
        /// <summary>
        /// Возвращает процент прочности пластин (0-100)
        /// </summary>
        public float GetArmorPercent()
        {
            float maxArmor = GetMaxArmor();
            if (maxArmor <= 0f) return 100f;
            return (Severity / maxArmor) * 100f;
        }

        public override bool ShouldRemove
        {
            get
            {
                // Никогда не удаляем hediff - он должен оставаться даже при severity = 0
                return false;
            }
        }
    }
}

