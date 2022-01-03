using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ExtendedPlatformPlacement
{
    public class EPPlayer : ModPlayer
    {
        public ExtendedPlatformPlacement Mod = ModContent.GetInstance<ExtendedPlatformPlacement>();
        public ExtensionMode EPMode = ExtensionMode.Auto;

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (ExtendedPlatformPlacement.SwitchExtensionModeHotkey.JustPressed)
            {
                ShiftMode();
                Main.NewText("Platform Mode Changed to " + EPMode.ToString());
            }
        }

        public void ShiftMode()
        {
            EPMode = (ExtensionMode)(((int)EPMode + 1) % ExtendedPlatformPlacement.extensionModes.Length);
        }
    }
}
