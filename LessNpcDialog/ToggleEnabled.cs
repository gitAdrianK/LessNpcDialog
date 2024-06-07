using JumpKing.PauseMenu.BT.Actions;

namespace LessNpcDialog
{
    public class ToggleEnabled : ITextToggle
    {
        public ToggleEnabled() : base(ModEntry.Preferences.IsEnabled)
        {
        }

        protected override string GetName() => "Disable NPC dialog";

        protected override void OnToggle()
        {
            ModEntry.Preferences.IsEnabled = !ModEntry.Preferences.IsEnabled;
        }
    }
}
