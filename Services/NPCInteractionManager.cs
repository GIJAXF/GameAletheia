using System;
using System.Collections.Generic;
using System.Linq;
using GameAletheiaCross.Models;

namespace GameAletheiaCross.Services
{
    public class NPCInteractionManager
    {
        private const float INTERACTION_DISTANCE = 50f;
        
        public NPC? FindNearestNPC(Player player, List<NPC> npcs)
        {
            if (npcs == null || npcs.Count == 0) return null;

            NPC? nearestNPC = null;
            float minDistance = float.MaxValue;

            foreach (var npc in npcs.Where(n => n.IsActive))
            {
                float distance = CalculateDistance(
                    player.Position.X, player.Position.Y,
                    npc.PositionX, npc.PositionY
                );

                if (distance < INTERACTION_DISTANCE && distance < minDistance)
                {
                    minDistance = distance;
                    nearestNPC = npc;
                }
            }

            return nearestNPC;
        }

        public bool IsNearNPC(Player player, List<NPC> npcs)
        {
            return FindNearestNPC(player, npcs) != null;
        }

        public bool IsNearSpecificNPC(Player player, NPC npc)
        {
            if (!npc.IsActive) return false;
            
            float distance = CalculateDistance(
                player.Position.X, player.Position.Y,
                npc.PositionX, npc.PositionY
            );

            return distance < INTERACTION_DISTANCE;
        }

        private float CalculateDistance(float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public string GetDialogueText(NPC npc, Player player)
        {
            string CurrentDialogue = string.Join("\n", npc.DialogueList);
            return CurrentDialogue;
        }

        public bool HasAvailableQuest(NPC npc)
        {
            return false;
        }

        public void MarkAsInteracted(NPC npc)
        {
            Console.WriteLine($"Interacción registrada con {npc.Name}");
        }
    }
}