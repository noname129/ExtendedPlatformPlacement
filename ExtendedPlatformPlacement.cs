using System;
using Terraria.ModLoader;

namespace ExtendedPlatformPlacement
{
	public enum ExtensionMode
	{
		Off,
		Auto,
		Horizontal,
		Upward,
		Downward
	}
	
	public class ExtendedPlatformPlacement : Mod
	{
		public static ModHotKey SwitchExtensionModeHotkey;
		public static ExtensionMode[] extensionModes = (ExtensionMode[])Enum.GetValues(typeof(ExtensionMode));

		public ExtendedPlatformPlacement()
        {
        }

        public override void Load()
        {
			SwitchExtensionModeHotkey = RegisterHotKey("Switch Extension Mode", "OemOpenBrackets");
        }

        public override void Unload()
        {
			SwitchExtensionModeHotkey = null;
		}
    }
}