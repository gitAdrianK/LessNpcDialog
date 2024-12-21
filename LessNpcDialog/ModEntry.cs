namespace LessNpcDialog
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using BehaviorTree;
    using HarmonyLib;
    using JumpKing.MiscEntities.OldMan;
    using JumpKing.Mods;
    using JumpKing.PauseMenu;

    [JumpKingMod(IDENTIFIER)]
    public static class ModEntry
    {
        private const string IDENTIFIER = "Zebra.LessNpcDialog";
        private const string HARMONY_IDENTIFIER = "Zebra.LessNpcDialog.Harmony";
        private const string SETTINGS_FILE = "Zebra.LessNpcDialog.Settings.xml";

        private static string AssemblyPath { get; set; }
        public static Preferences Preferences { get; private set; }

        [MainMenuItemSetting]
        [PauseMenuItemSetting]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for JK")]
        public static ToggleEnabled Toggle(object factory, GuiFormat format) => new ToggleEnabled();

        /// <summary>
        /// Called by Jump King before the level loads
        /// </summary>
        [BeforeLevelLoad]
        public static void BeforeLevelLoad()
        {
            AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try
            {
                Preferences = XmlSerializerHelper.Deserialize<Preferences>($@"{AssemblyPath}\{SETTINGS_FILE}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[ERROR] [{IDENTIFIER}] {e.Message}");
                Preferences = new Preferences();
            }
            Preferences.PropertyChanged += SaveSettingsOnFile;

            var harmony = new Harmony(HARMONY_IDENTIFIER);
            var sayLineMyRun = typeof(SayLine).GetMethod("MyRun", BindingFlags.NonPublic | BindingFlags.Instance);
            var preventTalk = new HarmonyMethod(typeof(ModEntry).GetMethod(nameof(PreventTalk)));
            _ = harmony.Patch(
                sayLineMyRun,
                prefix: preventTalk);
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Harmony naming convention")]
        public static bool PreventTalk(ref BTresult __result)
        {
            if (Preferences.IsEnabled)
            {
                __result = BTresult.Success;
                return false;
            }
            return true;
        }

        private static void SaveSettingsOnFile(object sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            try
            {
                XmlSerializerHelper.Serialize($@"{AssemblyPath}\{SETTINGS_FILE}", Preferences);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[ERROR] [{IDENTIFIER}] {e.Message}");
            }
        }
    }
}
