using System;
using System.Collections.Generic;
using GameAletheiaCross.Models;

namespace GameAletheiaCross.Services
{
    public class CollisionManager
    {
        public void CheckPlatformCollisions(Player player, List<Level.Platform> platforms)
        {
            if (platforms == null || platforms.Count == 0) return;

            bool onGround = false;

            foreach (var platform in platforms)
            {
                if (!platform.IsSolid) continue;

                if (IsCollidingWithPlatform(player, platform))
                {
                    if (player.Velocity.Y > 0 && 
                        player.Position.Y <= platform.Y)
                    {
                        player.Position.Y = platform.Y;
                        player.Velocity.Y = 0;
                        player.IsJumping = false;
                        onGround = true;
                    }
                }
            }

            if (!onGround && player.Position.Y < 500)
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

            return playerRight > platformLeft &&
                   playerLeft < platformRight &&
                   playerBottom > platformTop &&
                   playerTop < platformBottom;
        }
    }
}