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

        public override bool UseItem(Item item, Player player)
        {
            extendedThisFrame = false;
            if (player.whoAmI != Main.myPlayer)
            {
                return false;
            }
            EPPlayer modPlayer = player.GetModPlayer<EPPlayer>();
            if (modPlayer.EPMode == ExtensionMode.Off)
            {
                return false;
            }

            if (player.itemTime == 0)
            {
                var targetCoord = Main.MouseWorld.ToTileCoordinates();
                int targetX = targetCoord.X;
                int targetY = targetCoord.Y;

                Tile startTile = Framing.GetTileSafely(targetX, targetY);

                if (!startTile.active())
                {
                    return false;
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
                            return false;
                        }

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

                        if (resultTile.active())
                        {
                            if (modPlayer.EPMode == ExtensionMode.Horizontal || (modPlayer.EPMode == ExtensionMode.Auto && startTile.slope() == 0))
                            {
                                if (startTile.halfBrick())
                                {
                                    WorldGen.PoundTile(targetX, targetY);
                                    if (Main.netMode == NetmodeID.MultiplayerClient)
                                        NetMessage.SendData(MessageID.TileChange, -1, -1, null, 7, targetX, targetY, 1f);
                                };
                            }
                            else if (modPlayer.EPMode == ExtensionMode.Upward
                                || (modPlayer.EPMode == ExtensionMode.Auto && startTile.slope() == (player.direction == 1 ? 2 : 1)))
                            {
                                WorldGen.SlopeTile(targetX, targetY, player.direction == 1 ? 2 : 1);
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 14, targetX, targetY, resultTile.slope());
                            }
                            else if (modPlayer.EPMode == ExtensionMode.Downward
                                || (modPlayer.EPMode == ExtensionMode.Auto && startTile.slope() == (player.direction == 1 ? 1 : 2)))
                            {
                                WorldGen.SlopeTile(targetX, targetY, player.direction == 1 ? 1 : 2);
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 14, targetX, targetY, resultTile.slope());
                            }

                            //WorldGen.PlaceTile(targetX, targetY, item.createTile, false, false, player.whoAmI, item.placeStyle);
                            extendedThisFrame = true;
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
                            return false;
                        }

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
                        return true;
                    }
                }
            }
            return false;
        }

        public override float UseTimeMultiplier(Item item, Player player)
        {
            if (!extendedThisFrame)
            {
                return 1f;
            }
            EPConfig config = ModContent.GetInstance<EPConfig>();
            if (config.FasterPlacement)
            {
                return 5f;
            }

            return 1f;
        }

        public override float MeleeSpeedMultiplier(Item item, Player player)
        {
            if (!extendedThisFrame)
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
            if (Main.tile[targetX, targetY].active())
            {
                int targetTileID = item.createTile;
                int x = targetX;
                int y = targetY;
                Tile startTile = Framing.GetTileSafely(targetX, targetY);
                Tile prevTile = null;
                int reach = 0;

                TileObjectData tileData = TileObjectData.GetTileData(targetTileID, item.placeStyle);

                Tile nextTile = Framing.GetTileSafely(x, y);
                while (nextTile.active() && IsPlatform(nextTile.type)
                    && (reach <= 1 || nextTile.HasSameSlope(prevTile)) && !nextTile.lava())
                {
                    prevTile = nextTile;
                    if (modPlayer.EPMode == ExtensionMode.Horizontal || (modPlayer.EPMode == ExtensionMode.Auto && startTile.slope() == 0))
                    {
                        x += player.direction;
                    }
                    else if ((modPlayer.EPMode == ExtensionMode.Upward && startTile.slope() != (player.direction == 1 ? 1 : 2))
                            || (modPlayer.EPMode == ExtensionMode.Auto && startTile.slope() == (player.direction == 1 ? 2 : 1)))
                    {
                        x += player.direction;
                        y -= 1;
                    }
                    else if ((modPlayer.EPMode == ExtensionMode.Downward && startTile.slope() != (player.direction == 1 ? 2 : 1))
                        || (modPlayer.EPMode == ExtensionMode.Auto && startTile.slope() == (player.direction == 1 ? 1 : 2)))
                    {
                        x += player.direction;
                        if (reach != 0 || startTile.slope() != 0)
                        {
                            y += 1;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    nextTile = Framing.GetTileSafely(x, y);

                    if (nextTile == null || x < 0 || y < 0 || x >= Main.mapMaxX || y >= Main.mapMaxY)
                    {
                        return false;
                    }
                    ++reach;

                }

                if (!nextTile.active())
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
            if (Main.tile[targetX, targetY].active())
            {
                int targetTileID = item.createTile;
                int x = targetX;
                int y = targetY;
                Tile startTile = Framing.GetTileSafely(targetX, targetY);
                Tile prevTile = null;
                int reach = 0;
                if (startTile.type != TileID.MinecartTrack)
                {
                    return false;
                }

                TileObjectData tileData = TileObjectData.GetTileData(targetTileID, item.placeStyle);

                Tile nextTile = Framing.GetTileSafely(x, y);
                while (nextTile.active() && nextTile.type == TileID.MinecartTrack)
                {
                    prevTile = nextTile;
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
                        if (reach != 0 || startTile.slope() != 0)
                        {
                            y += 1;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    nextTile = Framing.GetTileSafely(x, y);

                    if (nextTile == null || x < 0 || y < 0 || x >= Main.mapMaxX || y >= Main.mapMaxY)
                    {
                        return false;
                    }
                    ++reach;

                }

                if (!nextTile.active())
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
