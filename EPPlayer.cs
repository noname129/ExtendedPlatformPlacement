using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ExtendedPlatformPlacement
{
    public class EPPlayer : ModPlayer
    {
        public ExtensionMode EPMode = ExtensionMode.Auto;

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (ExtendedPlatformPlacement.SwitchExtensionModeHotkey.JustPressed)
            {
                EPMode = (ExtensionMode)(((int)EPMode + 1) % 5);
                Main.NewText("Platform Mode Changed to " + EPMode.ToString());
            }
        }
    }
}
