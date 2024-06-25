using BehaviorTree;
using HarmonyLib;
using JumpKing.MiscEntities.OldMan;
using JumpKing.Mods;
using JumpKing.PauseMenu;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LessNpcDialog
{
    [JumpKingMod(IDENTIFIER)]
    public static class ModEntry
    {
        const string IDENTIFIER = "Zebra.LessNpcDialog";
        const string HARMONY_IDENTIFIER = "Zebra.LessNpcDialog.Harmony";
        const string SETTINGS_FILE = "Zebra.LessNpcDialog.Settings.xml";

        private static string AssemblyPath { get; set; }
        public static Preferences Preferences { get; private set; }

        [MainMenuItemSetting]
        [PauseMenuItemSetting]
        public static ToggleEnabled Toggle(object factory, GuiFormat format)
        {
            return new ToggleEnabled();
        }

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

            Harmony harmony = new Harmony(HARMONY_IDENTIFIER);
            MethodInfo sayLineMyRun = typeof(SayLine).GetMethod("MyRun", BindingFlags.NonPublic | BindingFlags.Instance);
            HarmonyMethod preventTalk = new HarmonyMethod(typeof(ModEntry).GetMethod(nameof(PreventTalk)));
            harmony.Patch(
                sayLineMyRun,
                prefix: preventTalk);
        }

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
