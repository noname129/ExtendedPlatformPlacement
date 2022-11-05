using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExtendedPlatformPlacement
{
    public class EPGlobalItem : GlobalItem
    {
        private static readonly EPConfig config = ModContent.GetInstance<EPConfig>();
        public static bool extendedThisFrame = false;

        public override bool? UseItem(Item item, Player player)
        {
            extendedThisFrame = false;
            EPPlayer modPlayer = player.GetModPlayer<EPPlayer>();
            if (!modPlayer.canExtend)
            {
                return null;
            }

            if (player.itemTime == 0)
            {
                if (!(player.position.X / 16f - (float)Player.tileRangeX - (float)player.inventory[player.selectedItem].tileBoost - (float)player.blockRange <= (float)Player.tileTargetX)
                    || !((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)player.inventory[player.selectedItem].tileBoost - 1f + (float)player.blockRange >= (float)Player.tileTargetX)
                    || !(player.position.Y / 16f - (float)Player.tileRangeY - (float)player.inventory[player.selectedItem].tileBoost - (float)player.blockRange <= (float)Player.tileTargetY)
                    || !((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY + (float)player.inventory[player.selectedItem].tileBoost - 2f + (float)player.blockRange >= (float)Player.tileTargetY))
                {
                    return null;
                }
                int oldTileRangeX = Player.tileRangeX;
                int oldTileRangeY = Player.tileRangeY;

                Player.tileRangeX = int.MaxValue / 32 - 20;
                Player.tileRangeY = int.MaxValue / 32 - 20;
                Player.tileTargetX = modPlayer.extendTargetX;
                Player.tileTargetY = modPlayer.extendTargetY;

                player.PlaceThing();

                Player.tileRangeX = oldTileRangeX;
                Player.tileRangeY = oldTileRangeY;

                if (IsPlatform(item))
                {
                    Tile resultTile = Framing.GetTileSafely(modPlayer.extendTargetX, modPlayer.extendTargetY);

                    if (resultTile.HasTile)
                    {
                        if (modPlayer.EPMode == ExtensionMode.Horizontal || (modPlayer.EPMode == ExtensionMode.Auto && modPlayer.startTile.Slope == SlopeType.Solid))
                        {
                            if (modPlayer.startTile.IsHalfBlock)
                            {
                                WorldGen.PoundTile(modPlayer.extendTargetX, modPlayer.extendTargetY);
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 7, modPlayer.extendTargetX, modPlayer.extendTargetY, 1f);
                            };
                        }
                        else if (modPlayer.EPMode == ExtensionMode.Upward
                            || (modPlayer.EPMode == ExtensionMode.Auto && (int)modPlayer.startTile.Slope == (player.direction == 1 ? 2 : 1)))
                        {
                            WorldGen.SlopeTile(modPlayer.extendTargetX, modPlayer.extendTargetY, player.direction == 1 ? 2 : 1);
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, modPlayer.extendTargetX, modPlayer.extendTargetY, (int)resultTile.Slope);
                        }
                        else if (modPlayer.EPMode == ExtensionMode.Downward
                            || (modPlayer.EPMode == ExtensionMode.Auto && (int)modPlayer.startTile.Slope == (player.direction == 1 ? 1 : 2)))
                        {
                            WorldGen.SlopeTile(modPlayer.extendTargetX, modPlayer.extendTargetY, player.direction == 1 ? 1 : 2);
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, modPlayer.extendTargetX, modPlayer.extendTargetY, (int)resultTile.Slope);
                        }

                        //WorldGen.PlaceTile(targetX, targetY, item.createTile, false, false, player.whoAmI, item.placeStyle);
                        extendedThisFrame = true;
                        return true;
                    }
                }
            }
            return null;
        }

        public override float UseSpeedMultiplier(Item item, Player player)
        {
            if (!extendedThisFrame || player.whoAmI != Main.myPlayer)
            {
                return 1f;
            }
            EPConfig config = ModContent.GetInstance<EPConfig>();
            if (config.FasterPlacement)
            {
                extendedThisFrame = false;
                return 5f;
            }

            return 1f;
        }

        internal static bool IsPlatform(Item item)
        {
            return IsPlatform(item.createTile);
        }

        internal static bool IsPlatform(int tile)
        {
            EPConfig Config = ModContent.GetInstance<EPConfig>();
            return tile != -1 && (TileID.Sets.Platforms[tile]
                || (Config.SmartPlatformCheck && IsPlatformStrict(tile)));
        }

        internal static bool IsPlatformStrict(int tile)
        {
            return tile != -1
                && Main.tileSolid[tile]
                && Main.tileSolidTop[tile]
                && Main.tileTable[tile]
            // maybe there're more conditions? idk
            ;
        }
    }
}
