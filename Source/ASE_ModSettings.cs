using Verse;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Настройки мода Android Subroutines Expanded.
    /// Сохраняются между сессиями игры.
    /// </summary>
    public class ASE_ModSettings : ModSettings
    {
        // ===== НАСТРОЙКИ ГЕЙМПЛЕЯ =====

        /// <summary>
        /// Сколько waste packs в день производит Vanometric Reactor (1..50).
        /// </summary>
        public int wastePacksPerDay = 7;

        // ===== НАСТРОЙКИ ЛОГИРОВАНИЯ (в консоль) =====
        
        /// <summary>
        /// Включить логирование для Experimental Void Reactor
        /// </summary>
        public bool enableVoidReactorLogging = true;
        
        /// <summary>
        /// Включить логирование для Vanometric Reactor
        /// </summary>
        public bool enableVanometricReactorLogging = true;
        
        /// <summary>
        /// Включить логирование урона в ближнем бою
        /// </summary>
        public bool enableMeleeDamageLogging = false;
        
        /// <summary>
        /// Включить логирование урона в дальнем бою
        /// </summary>
        public bool enableRangedDamageLogging = false;

        /// <summary>
        /// Включить логирование входящего урона (IncomingDamageFactor)
        /// </summary>
        public bool enableIncomingDamageLogging = false;
        
        /// <summary>
        /// Включить общее логирование мода
        /// </summary>
        public bool enableGeneralLogging = true;
        
        /// <summary>
        /// Включить логирование для Productivity Protocol Mk.III
        /// </summary>
        public bool enableProductivityProtocolLogging = true;
        
        /// <summary>
        /// Включить логирование для Reactive Armor
        /// </summary>
        public bool enableReactiveArmorLogging = true;
        
        /// <summary>
        /// Включить логирование для Deflector System
        /// </summary>
        public bool enableDeflectorSystemLogging = true;
        
        /// <summary>
        /// Включить логирование для Anti-Insectoid Plates
        /// </summary>
        public bool enableAntiInsectoidPlatesLogging = true;
        
        /// <summary>
        /// Включить логирование для Berserker Protocol
        /// </summary>
        public bool enableBerserkerProtocolLogging = true;
        
        /// <summary>
        /// Включить логирование для Unarmored Evasion
        /// </summary>
        public bool enableUnarmoredEvasionLogging = true;
        
        /// <summary>
        /// Включить логирование для Scrap Warrior
        /// </summary>
        public bool enableScrapWarriorLogging = true;
        
        // ===== НАСТРОЙКИ ИГРОВЫХ УВЕДОМЛЕНИЙ (в левом верхнем углу) =====
        
        /// <summary>
        /// Показывать игровые уведомления для Void Reactor
        /// </summary>
        public bool enableVoidReactorMessages = true;
        
        /// <summary>
        /// Показывать игровые уведомления для Vanometric Reactor
        /// </summary>
        public bool enableVanometricReactorMessages = true;
        
        /// <summary>
        /// Показывать игровые уведомления для Productivity Protocol Mk.III
        /// </summary>
        public bool enableProductivityProtocolMessages = true;
        
        /// <summary>
        /// Показывать игровые уведомления для Deep Mining Sensors
        /// </summary>
        public bool enableDeepMiningSensorsMessages = true;
        
        /// <summary>
        /// Показывать игровые уведомления для Shredder Core
        /// </summary>
        public bool enableShredderCoreMessages = true;
        
        /// <summary>
        /// Показывать игровые уведомления для Smelter Core
        /// </summary>
        public bool enableSmelterCoreMessages = true;
        
        /// <summary>
        /// Показывать игровые уведомления для Reactive Armor
        /// </summary>
        public bool enableReactiveArmorMessages = true;
        
        /// <summary>
        /// Показывать игровые уведомления для Deflector System
        /// </summary>
        public bool enableDeflectorSystemMessages = true;
        
        /// <summary>
        /// Показывать игровые уведомления для Anti-Insectoid Plates
        /// </summary>
        public bool enableAntiInsectoidPlatesMessages = true;
        
        /// <summary>
        /// Показывать игровые уведомления для Combat Data Logger
        /// </summary>
        public bool enableCombatDataLoggerMessages = true;

        /// <summary>
        /// Сохранение/загрузка настроек
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            // Геймплей
            Scribe_Values.Look(ref wastePacksPerDay, "wastePacksPerDay", 7);
            // Логирование
            Scribe_Values.Look(ref enableVoidReactorLogging, "enableVoidReactorLogging", true);
            Scribe_Values.Look(ref enableVanometricReactorLogging, "enableVanometricReactorLogging", true);
            Scribe_Values.Look(ref enableMeleeDamageLogging, "enableMeleeDamageLogging", false);
            Scribe_Values.Look(ref enableRangedDamageLogging, "enableRangedDamageLogging", false);
            Scribe_Values.Look(ref enableIncomingDamageLogging, "enableIncomingDamageLogging", false);
            Scribe_Values.Look(ref enableGeneralLogging, "enableGeneralLogging", true);
            Scribe_Values.Look(ref enableProductivityProtocolLogging, "enableProductivityProtocolLogging", true);
            Scribe_Values.Look(ref enableReactiveArmorLogging, "enableReactiveArmorLogging", true);
            Scribe_Values.Look(ref enableDeflectorSystemLogging, "enableDeflectorSystemLogging", true);
            Scribe_Values.Look(ref enableAntiInsectoidPlatesLogging, "enableAntiInsectoidPlatesLogging", true);
            Scribe_Values.Look(ref enableBerserkerProtocolLogging, "enableBerserkerProtocolLogging", true);
            Scribe_Values.Look(ref enableUnarmoredEvasionLogging, "enableUnarmoredEvasionLogging", true);
            Scribe_Values.Look(ref enableScrapWarriorLogging, "enableScrapWarriorLogging", true);
            // Игровые уведомления
            Scribe_Values.Look(ref enableVoidReactorMessages, "enableVoidReactorMessages", true);
            Scribe_Values.Look(ref enableVanometricReactorMessages, "enableVanometricReactorMessages", true);
            Scribe_Values.Look(ref enableProductivityProtocolMessages, "enableProductivityProtocolMessages", true);
            Scribe_Values.Look(ref enableDeepMiningSensorsMessages, "enableDeepMiningSensorsMessages", true);
            Scribe_Values.Look(ref enableShredderCoreMessages, "enableShredderCoreMessages", true);
            Scribe_Values.Look(ref enableSmelterCoreMessages, "enableSmelterCoreMessages", true);
            Scribe_Values.Look(ref enableReactiveArmorMessages, "enableReactiveArmorMessages", true);
            Scribe_Values.Look(ref enableDeflectorSystemMessages, "enableDeflectorSystemMessages", true);
            Scribe_Values.Look(ref enableAntiInsectoidPlatesMessages, "enableAntiInsectoidPlatesMessages", true);
            Scribe_Values.Look(ref enableCombatDataLoggerMessages, "enableCombatDataLoggerMessages", true);
        }
    }
}
