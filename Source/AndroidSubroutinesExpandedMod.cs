using HarmonyLib;
using Verse;

namespace AndroidSubroutinesExpanded
{
    /// <summary>
    /// Главный класс мода с поддержкой настроек
    /// </summary>
    public class AndroidSubroutinesExpandedMod : Mod
    {
        /// <summary>
        /// Статический доступ к настройкам мода
        /// </summary>
        public static ASE_ModSettings Settings;

        public AndroidSubroutinesExpandedMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ASE_ModSettings>();
            
            // Инициализируем Harmony
            var harmony = new Harmony("AndroidSubroutinesExpanded.Core");
            harmony.PatchAll();
            
            // Регистрируем патч для BehavioristStation вручную, чтобы избежать вызова статического конструктора
            ASE_BehavioristStationPatch.RegisterPatch(harmony);
            
            Log.Message("[ASE] Android Subroutines Expanded: Mod loaded successfully!");
        }

        /// <summary>
        /// Название категории в меню настроек
        /// </summary>
        public override string SettingsCategory()
        {
            return "Android Subroutines Expanded";
        }

        /// <summary>
        /// Содержимое окна настроек
        /// </summary>
        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            
            // === Секция логирования ===
            listing.Label("=== Console Logging (Developer Log) ===");
            listing.Gap(6f);
            
            listing.CheckboxLabeled(
                "Void Reactor console logging", 
                ref Settings.enableVoidReactorLogging, 
                "Log to console when mechanoid raids are triggered");
            
            listing.CheckboxLabeled(
                "Vanometric Reactor console logging", 
                ref Settings.enableVanometricReactorLogging, 
                "Log to console when waste is generated");
            
            listing.CheckboxLabeled(
                "Melee damage logging", 
                ref Settings.enableMeleeDamageLogging, 
                "Log detailed melee damage (can be spammy)");
            
            listing.CheckboxLabeled(
                "Incoming damage logging", 
                ref Settings.enableIncomingDamageLogging, 
                "Log incoming damage with IncomingDamageFactor (Heavy Plating, Narcotic Harvester, etc.)");
            
            listing.CheckboxLabeled(
                "General mod logging", 
                ref Settings.enableGeneralLogging, 
                "Log gene activation, hediff creation, etc.");
            
            listing.CheckboxLabeled(
                "Productivity Protocol Mk.III logging", 
                ref Settings.enableProductivityProtocolLogging, 
                "Log temperature-based work speed changes");
            
            listing.CheckboxLabeled(
                "Reactive Armor logging", 
                ref Settings.enableReactiveArmorLogging, 
                "Log melee damage reflection");
            
            listing.CheckboxLabeled(
                "Deflector System logging", 
                ref Settings.enableDeflectorSystemLogging, 
                "Log ranged damage reflection (50% chance)");
            
            listing.CheckboxLabeled(
                "Anti-Insectoid Plates logging", 
                ref Settings.enableAntiInsectoidPlatesLogging, 
                "Log ablative armor damage absorption");
            
            listing.CheckboxLabeled(
                "Berserker Protocol logging", 
                ref Settings.enableBerserkerProtocolLogging, 
                "Log HP-based damage bonus changes");
            
            listing.CheckboxLabeled(
                "Unarmored Evasion logging", 
                ref Settings.enableUnarmoredEvasionLogging, 
                "Log empty slot dodge bonus calculations");
            
            listing.CheckboxLabeled(
                "Scrap Warrior logging", 
                ref Settings.enableScrapWarriorLogging, 
                "Log market value damage bonus calculations");
            
            listing.Gap(12f);
            
            // === Секция игровых уведомлений ===
            listing.Label("=== In-Game Messages (Top-Left Corner) ===");
            listing.Gap(6f);
            
            listing.CheckboxLabeled(
                "Void Reactor attack messages", 
                ref Settings.enableVoidReactorMessages, 
                "Show message when mechanoids are attracted");
            
            listing.CheckboxLabeled(
                "Vanometric Reactor waste messages", 
                ref Settings.enableVanometricReactorMessages, 
                "Show message when waste is produced");
            
            listing.CheckboxLabeled(
                "Productivity Protocol Mk.III messages", 
                ref Settings.enableProductivityProtocolMessages, 
                "Show temperature adaptation messages");
            
            listing.CheckboxLabeled(
                "Deep Mining Sensors messages", 
                ref Settings.enableDeepMiningSensorsMessages, 
                "Show messages when bonus resources are found");
            
            listing.CheckboxLabeled(
                "Shredder Core messages", 
                ref Settings.enableShredderCoreMessages, 
                "Show messages when bonus materials are recovered");
            
            listing.CheckboxLabeled(
                "Smelter Core messages", 
                ref Settings.enableSmelterCoreMessages, 
                "Show messages when bonus metals are recovered");
            
            listing.CheckboxLabeled(
                "Reactive Armor messages", 
                ref Settings.enableReactiveArmorMessages, 
                "Show messages when melee damage is reflected");
            
            listing.CheckboxLabeled(
                "Deflector System messages", 
                ref Settings.enableDeflectorSystemMessages, 
                "Show messages when ranged damage is reflected");
            
            listing.CheckboxLabeled(
                "Anti-Insectoid Plates messages", 
                ref Settings.enableAntiInsectoidPlatesMessages, 
                "Show messages when armor absorbs damage");
            
            listing.Gap(12f);
            listing.Label("Disable options to reduce spam during gameplay.");
            
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
    
    /// <summary>
    /// Статический конструктор для совместимости
    /// </summary>
    [StaticConstructorOnStartup]
    public static class AndroidSubroutinesExpandedStartup
    {
        static AndroidSubroutinesExpandedStartup()
        {
            if (AndroidSubroutinesExpandedMod.Settings == null)
            {
                Log.Warning("[ASE] Settings not loaded through Mod class");
            }
        }
    }
}
