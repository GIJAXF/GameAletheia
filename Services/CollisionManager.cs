using System;
using System.Collections.Generic;
using GameAletheiaCross.Models;

namespace GameAletheiaCross.Services
{
    public class CollisionManager
    {
        public void CheckPlatformCollisions(Player player, List<Level.Platform> platforms, Level.Floor floor = null)
        {
            bool onGround = false;

            // ⭐ VERIFICAR COLISIÓN CON EL PISO PRINCIPAL
            if (floor != null && floor.IsSolid)
            {
                if (IsCollidingWithFloor(player, floor))
                {
                    if (player.Velocity.Y >= 0 && player.Position.Y <= floor.Y + floor.Height)
                    {
                        player.Position.Y = floor.Y;
                        player.Velocity.Y = 0;
                        player.IsJumping = false;
                        onGround = true;
                    }
                }
            }

            // Verificar colisiones con plataformas
            if (platforms != null && platforms.Count > 0)
            {
                foreach (var platform in platforms)
                {
                    if (!platform.IsSolid) continue;

                    if (IsCollidingWithPlatform(player, platform))
                    {
                        if (player.Velocity.Y >= 0 && player.Position.Y <= platform.Y + platform.Height)
                        {
                            player.Position.Y = platform.Y;
                            player.Velocity.Y = 0;
                            player.IsJumping = false;
                            onGround = true;
                        }
                    }
                }
            }

            if (!onGround)
            {
                player.IsJumping = true;
            }
        }

        private bool IsCollidingWithPlatform(Player player, Level.Platform platform)
        {
            float playerLeft = player.Position.X - 20;
            float playerRight = player.Position.X + 20;
            float playerTop = player.Position.Y - 60;
            float playerBottom = player.Position.Y;

            float platformLeft = platform.X;
            float platformRight = platform.X + platform.Width;
            float platformTop = platform.Y;
            float platformBottom = platform.Y + platform.Height;

            bool colliding = playerRight > platformLeft &&
                           playerLeft < platformRight &&
                           playerBottom > platformTop &&
                           playerTop < platformBottom;

            return colliding;
        }

        private bool IsCollidingWithFloor(Player player, Level.Floor floor)
        {
            float playerLeft = player.Position.X - 20;
            float playerRight = player.Position.X + 20;
            float playerTop = player.Position.Y - 60;
            float playerBottom = player.Position.Y;

            float floorLeft = floor.X;
            float floorRight = floor.X + floor.Width;
            float floorTop = floor.Y;
            float floorBottom = floor.Y + floor.Height;

            bool colliding = playerRight > floorLeft &&
                           playerLeft < floorRight &&
                           playerBottom > floorTop &&
                           playerTop < floorBottom;

            return colliding;
        }
    }
}