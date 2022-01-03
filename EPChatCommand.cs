using Terraria;
using Terraria.ModLoader;

namespace ExtendedPlatformPlacement
{
    public class EPChatCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "epmode";

        public override string Usage => "/epmode or /epmode [mode name]";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<EPPlayer>();
            if (args.Length == 0)
            {
                modPlayer.ShiftMode();
                Main.NewText("Platform Mode Changed to " + modPlayer.EPMode.ToString());
            }
            else if (args.Length == 1)
            {
                foreach (var mode in ExtendedPlatformPlacement.extensionModes)
                {
                    if (args[0] == ((int)mode).ToString() || args[0].ToLower() == mode.ToString().ToLower())
                    {
                        modPlayer.EPMode = mode;
                    }
                }
                Main.NewText("Platform Mode Changed to " + modPlayer.EPMode.ToString());
            }
        }
    }
}
