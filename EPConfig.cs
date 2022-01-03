using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ExtendedPlatformPlacement
{
    class EPConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        [Label("Smart Platform Check")]
        [Tooltip("Check if the tile behaves like platforms if it's not known as a platform if set to true.\n" +
            "If so, it is also affected.\n" +
            "Can work with mod platforms or planter boxes, but may cause unexpected problems.")]
        public bool SmartPlatformCheck = true;

        [DefaultValue(false)]
        [Label("Faster Placement")]
        [Tooltip("Place platform faster when extending it")]
        public bool FasterPlacement = false;
    }
}
