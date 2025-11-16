using System;
using System.Collections.Generic;
using GameAletheiaCross.Models;

namespace GameAletheiaCross.Services
{
    public class CollisionManager
    {
public void CheckPlatformCollisions(Player player, List<Level.Platform> platforms, Level.Platform? floor)
        {
            bool onGround = false;

            // ✅ VERIFICAR COLISIÓN CON EL PISO PRIMERO
            if (floor != null && floor.IsSolid)
            {
                if (IsCollidingWithPlatform(player, floor))
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

            // ✅ VERIFICAR COLISIONES CON PLATAFORMAS (solo si no está en el suelo)
            if (!onGround && platforms != null && platforms.Count > 0)
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
                            break; // Salir del loop cuando encuentra una plataforma
                        }
                    }
                }
            }

            // Si NO está en el suelo, debe estar saltando
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