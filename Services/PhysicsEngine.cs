using GameAletheiaCross.Models;

namespace GameAletheiaCross.Services
{
    public class PhysicsEngine
    {
        private const float GRAVITY = 0.8f;
        private const float MAX_FALL_SPEED = 20f;
        
        public void ApplyGravity(Player player)
        {
            player.Velocity.Y += GRAVITY;
            
            if (player.Velocity.Y > MAX_FALL_SPEED)
                player.Velocity.Y = MAX_FALL_SPEED;
        }
        
        public void UpdatePosition(Player player)
        {
            player.Position.X += player.Velocity.X;
            player.Position.Y += player.Velocity.Y;
        }
    }
}