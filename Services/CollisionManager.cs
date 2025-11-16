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

                //  VERIFICAR COLISIÓN CON ESTA PLATAFORMA
                if (IsCollidingWithPlatform(player, platform))
                {
                    //  El jugador está cayendo Y está ENCIMA de la plataforma
                    if (player.Velocity.Y >= 0 && player.Position.Y <= platform.Y + platform.Height)
                    {
                        // Posicionar el jugador SOBRE la plataforma
                        player.Position.Y = platform.Y;
                        player.Velocity.Y = 0;
                        player.IsJumping = false;
                        onGround = true;
                    }
                }
            }

            //  Si NO está en el suelo, debe estar saltando
            if (!onGround)
            {
                player.IsJumping = true;
            }
        }

        private bool IsCollidingWithPlatform(Player player, Level.Platform platform)
        {
            //   Dimensiones del jugador (rectángulo de colisión)
            // El jugador se dibuja con X-20 a X+20 (ancho 40)
            // Y con Y-60 a Y (alto 60)
            float playerLeft = player.Position.X - 20;
            float playerRight = player.Position.X + 20;
            float playerTop = player.Position.Y - 60;
            float playerBottom = player.Position.Y;

            //  ️ Dimensiones de la plataforma
            float platformLeft = platform.X;
            float platformRight = platform.X + platform.Width;
            float platformTop = platform.Y;
            float platformBottom = platform.Y + platform.Height;

            //Console.WriteLine($"DEBUG Colisión:");
            //Console.WriteLine($"  Jugador: ({playerLeft}, {playerTop}) a ({playerRight}, {playerBottom})");
            //Console.WriteLine($"  Plataforma: ({platformLeft}, {platformTop}) a ({platformRight}, {platformBottom})");

            //  AABB Collision Detection (Axis-Aligned Bounding Box)
            // Dos rectángulos colisionan si se solapan en ambos ejes
            bool colliding = playerRight > platformLeft &&
                           playerLeft < platformRight &&
                           playerBottom > platformTop &&
                           playerTop < platformBottom;

            if (colliding)
            {
            //    Console.WriteLine($"   ¡COLISIÓN DETECTADA!");
            }

            return colliding;
        }
    }
}