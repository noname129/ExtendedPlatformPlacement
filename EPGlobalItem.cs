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
        public static bool alreadyHooked = false;

        public override bool? UseItem(Item item, Player player)
        {   
            if (alreadyHooked)
            {
                alreadyHooked = false;
                return true;
            }
            extendedThisFrame = false;
            if (player.whoAmI != Main.myPlayer)
            {
                return null;
            }
            EPPlayer modPlayer = player.GetModPlayer<EPPlayer>();
            if (modPlayer.EPMode == ExtensionMode.Off)
            {
                return null;
            }

            var targetCoord = Main.MouseWorld.ToTileCoordinates();
            int targetX = targetCoord.X;
            int targetY = targetCoord.Y;

            Tile startTile = Framing.GetTileSafely(targetX, targetY);

            if (!startTile.HasTile)
            {
                return null;
            }

            if (IsPlatform(item))
            {
                if (CheckExtensibility(item, player, modPlayer.EPMode, ref targetX, ref targetY))
                {
                    if (!(player.position.X / 16f - (float)Player.tileRangeX - (float)player.inventory[player.selectedItem].tileBoost - (float)player.blockRange <= (float)Player.tileTargetX)
                    || !((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)player.inventory[player.selectedItem].tileBoost - 1f + (float)player.blockRange >= (float)Player.tileTargetX)
                    || !(player.position.Y / 16f - (float)Player.tileRangeY - (float)player.inventory[player.selectedItem].tileBoost - (float)player.blockRange <= (float)Player.tileTargetY)
                    || !((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY + (float)player.inventory[player.selectedItem].tileBoost - 2f + (float)player.blockRange >= (float)Player.tileTargetY))
                    {
                        return null;
                    }

                    alreadyHooked = true;
                    int oldTileRangeX = Player.tileRangeX;
                    int oldTileRangeY = Player.tileRangeY;

                    Player.tileRangeX = int.MaxValue / 32 - 20;
                    Player.tileRangeY = int.MaxValue / 32 - 20;
                    Player.tileTargetX = targetX;
                    Player.tileTargetY = targetY;

                    player.PlaceThing();

                    Player.tileRangeX = oldTileRangeX;
                    Player.tileRangeY = oldTileRangeY;

                    Tile resultTile = Framing.GetTileSafely(targetX, targetY);

                    if (resultTile.HasTile)
                    {
                        if (modPlayer.EPMode == ExtensionMode.Horizontal || (modPlayer.EPMode == ExtensionMode.Auto && startTile.Slope == SlopeType.Solid))
                        {
                            if (startTile.IsHalfBlock)
                            {
                                WorldGen.PoundTile(targetX, targetY);
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 7, targetX, targetY, 1f);
                            };
                        }
                        else if (modPlayer.EPMode == ExtensionMode.Upward
                            || (modPlayer.EPMode == ExtensionMode.Auto && (int)startTile.Slope == (player.direction == 1 ? 2 : 1)))
                        {
                            WorldGen.SlopeTile(targetX, targetY, player.direction == 1 ? 2 : 1);
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, targetX, targetY, (int)resultTile.Slope);
                        }
                        else if (modPlayer.EPMode == ExtensionMode.Downward
                            || (modPlayer.EPMode == ExtensionMode.Auto && (int)startTile.Slope == (player.direction == 1 ? 1 : 2)))
                        {
                            WorldGen.SlopeTile(targetX, targetY, player.direction == 1 ? 1 : 2);
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, targetX, targetY, (int)resultTile.Slope);
                        }

                        //WorldGen.PlaceTile(targetX, targetY, item.createTile, false, false, player.whoAmI, item.placeStyle);
                        extendedThisFrame = true;
                        alreadyHooked = false;
                        return true;
                    }
                }
            }
            else if (config.ApplyMinecartTracks && item.createTile == TileID.MinecartTrack)
            {
                if (CheckTrackExtensibility(item, player, modPlayer.EPMode, ref targetX, ref targetY))
                {
                    if (!(player.position.X / 16f - (float)Player.tileRangeX - (float)player.inventory[player.selectedItem].tileBoost - (float)player.blockRange <= (float)Player.tileTargetX)
                    || !((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)player.inventory[player.selectedItem].tileBoost - 1f + (float)player.blockRange >= (float)Player.tileTargetX)
                    || !(player.position.Y / 16f - (float)Player.tileRangeY - (float)player.inventory[player.selectedItem].tileBoost - (float)player.blockRange <= (float)Player.tileTargetY)
                    || !((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY + (float)player.inventory[player.selectedItem].tileBoost - 2f + (float)player.blockRange >= (float)Player.tileTargetY))
                    {
                        return null;
                    }

                    alreadyHooked = true;

                    int oldTileRangeX = Player.tileRangeX;
                    int oldTileRangeY = Player.tileRangeY;

                    Player.tileRangeX = int.MaxValue / 32 - 20;
                    Player.tileRangeY = int.MaxValue / 32 - 20;
                    Player.tileTargetX = targetX;
                    Player.tileTargetY = targetY;

                    player.PlaceThing();

                    Player.tileRangeX = oldTileRangeX;
                    Player.tileRangeY = oldTileRangeY;

                    extendedThisFrame = true;
                    alreadyHooked = false;
                    return true;
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

        private bool IsPlatform(Item item)
        {
            return IsPlatform(item.createTile);
        }

        private bool IsPlatform(int tile)
        {
            EPConfig Config = ModContent.GetInstance<EPConfig>();
            return tile != -1 && (TileID.Sets.Platforms[tile]
                || (Config.SmartPlatformCheck && IsPlatformStrict(tile)));
        }

        private bool IsPlatformStrict(int tile)
        {
            return tile != -1
                && Main.tileSolid[tile]
                && Main.tileSolidTop[tile]
                && Main.tileTable[tile]
            // maybe there're more conditions? idk
            ;
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
                while (nextTile.HasTile && IsPlatform(nextTile.TileType)
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

            //if (Main.tileRope[inventory[selectedItem].createTile] && canUse && Main.tile[tileTargetX, tileTargetY].active() && Main.tileRope[Main.tile[tileTargetX, tileTargetY].type])
            //{
            //    int num = tileTargetY;
            //    int num2 = tileTargetX;
            //    _ = inventory[selectedItem].createTile;
            //    while (Main.tile[num2, num].active() && Main.tileRope[Main.tile[num2, num].type] && num < Main.maxTilesX - 5 && Main.tile[num2, num + 2] != null && !Main.tile[num2, num + 1].lava())
            //    {
            //        num++;
            //        if (Main.tile[num2, num] == null)
            //        {
            //            canUse = false;
            //            num = tileTargetY;
            //            break;
            //        }
            //    }

            //    if (!Main.tile[num2, num].active())
            //        tileTargetY = num;
            //}

            //return canUse;
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
    }
}
