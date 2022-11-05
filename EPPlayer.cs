using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using tModPorter;

namespace ExtendedPlatformPlacement
{
    public class EPPlayer : ModPlayer
    {
        public ExtendedPlatformPlacement EPMod = ModContent.GetInstance<ExtendedPlatformPlacement>();
        public ExtensionMode EPMode = ExtensionMode.Auto;
        private static readonly EPConfig config = ModContent.GetInstance<EPConfig>();

        internal bool canExtend = false;
        internal int extendTargetX, extendTargetY;
        internal Tile startTile;

        public override bool PreItemCheck()
        {
            canExtend = false;

            bool isLocalPlayer = Player.whoAmI == Main.myPlayer;
            bool isModeOn = EPMode != ExtensionMode.Off;
            Item item = Player.inventory[Player.selectedItem];

            if (isLocalPlayer && isModeOn)
            {
                var targetCoord = Main.MouseWorld.ToTileCoordinates();
                extendTargetX = targetCoord.X;
                extendTargetY = targetCoord.Y;
                startTile = Framing.GetTileSafely(extendTargetX, extendTargetY);

                if (EPGlobalItem.IsPlatform(item))
                {
                    if (startTile.HasTile && CheckExtensibility(item, Player, EPMode, ref extendTargetX, ref extendTargetY))
                    {
                        canExtend = true;
                    }
                }
                else if (config.ApplyMinecartTracks && item.createTile == TileID.MinecartTrack)
                {
                    if (startTile.HasTile && CheckTrackExtensibility(item, Player, EPMode, ref extendTargetX, ref extendTargetY))
                    {
                        canExtend = true;
                    }
                }
            }
            return true;
        }

        public override void PostItemCheck()
        {
            canExtend = false;
        }

        private bool CheckExtensibility(Item item, Player player, ExtensionMode mode, ref int targetX, ref int targetY)
        {
            EPPlayer modPlayer = player.GetModPlayer<EPPlayer>();
            if (Main.tile[targetX, targetY].HasTile)
            {
                int targetTileID = item.createTile;
                int x = targetX;
                int y = targetY;
                Tile startTile = Framing.GetTileSafely(targetX, targetY);
                Tile? prevTile = null;
                int reach = 0;

                TileObjectData tileData = TileObjectData.GetTileData(targetTileID, item.placeStyle);

                Tile nextTile = Framing.GetTileSafely(x, y);
                while (nextTile.HasTile && EPGlobalItem.IsPlatform(nextTile.TileType)
                    && (reach <= 1 || nextTile.BlockType == prevTile?.BlockType) && nextTile.LiquidType != LiquidID.Lava)
                {
                    prevTile = nextTile;
                    if (modPlayer.EPMode == ExtensionMode.Horizontal || (modPlayer.EPMode == ExtensionMode.Auto && startTile.Slope == SlopeType.Solid))
                    {
                        x += player.direction;
                    }
                    else if ((modPlayer.EPMode == ExtensionMode.Upward && startTile.Slope != (player.direction == 1 ? SlopeType.SlopeDownLeft : SlopeType.SlopeDownRight))
                            || (modPlayer.EPMode == ExtensionMode.Auto && startTile.Slope == (player.direction == 1 ? SlopeType.SlopeDownRight : SlopeType.SlopeDownLeft)))
                    {
                        x += player.direction;
                        y -= 1;
                    }
                    else if ((modPlayer.EPMode == ExtensionMode.Downward && startTile.Slope != (player.direction == 1 ? SlopeType.SlopeDownRight : SlopeType.SlopeDownLeft))
                        || (modPlayer.EPMode == ExtensionMode.Auto && startTile.Slope == (player.direction == 1 ? SlopeType.SlopeDownLeft : SlopeType.SlopeDownRight)))
                    {
                        x += player.direction;
                        if (reach != 0 || (int)startTile.Slope != 0)
                        {
                            y += 1;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    nextTile = Framing.GetTileSafely(x, y);

                    if (nextTile == null || x < 5 || y < 5 || x >= Main.maxTilesX || y >= Main.maxTilesY)
                    {
                        return false;
                    }
                    ++reach;

                }

                if (!nextTile.HasTile)
                {
                    targetX = x;
                    targetY = y;
                    return true;
                }
            }
            return false;
        }

        private bool CheckTrackExtensibility(Item item, Player player, ExtensionMode mode, ref int targetX, ref int targetY)
        {
            EPPlayer modPlayer = player.GetModPlayer<EPPlayer>();
            if (Main.tile[targetX, targetY].HasTile)
            {
                int targetTileID = item.createTile;
                int x = targetX;
                int y = targetY;
                Tile startTile = Framing.GetTileSafely(targetX, targetY);
                int reach = 0;
                if (startTile.TileType != TileID.MinecartTrack)
                {
                    return false;
                }

                Tile nextTile = Framing.GetTileSafely(x, y);
                while (nextTile.HasTile && nextTile.TileType == TileID.MinecartTrack)
                {
                    if (modPlayer.EPMode == ExtensionMode.Horizontal || modPlayer.EPMode == ExtensionMode.Auto)
                    {
                        x += player.direction;
                    }
                    else if (modPlayer.EPMode == ExtensionMode.Upward)
                    {
                        x += player.direction;
                        y -= 1;
                    }
                    else if (modPlayer.EPMode == ExtensionMode.Downward)
                    {
                        x += player.direction;
                        if (reach != 0 || startTile.Slope != SlopeType.Solid)
                        {
                            y += 1;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    nextTile = Framing.GetTileSafely(x, y);

                    if (nextTile == null || x < 5 || y < 5 || x >= Main.maxTilesX || y >= Main.maxTilesY)
                    {
                        return false;
                    }
                    ++reach;

                }

                if (!nextTile.HasTile)
                {
                    targetX = x;
                    targetY = y;
                    return true;
                }
            }
            return false;
        }

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
