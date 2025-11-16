// Crea este archivo: Services/UpdateFactionsScript.cs
// Y ejecútalo una vez para actualizar las facciones en MongoDB

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using MongoDB.Driver;

namespace GameAletheiaCross.Services
{
    public class UpdateFactionsScript
    {
        public static async Task UpdateFactionsAsync()
        {
            Console.WriteLine("\n🔄 Actualizando facciones en la base de datos...");
            
            var dbService = new MongoDbService();
            var factionsCollection = dbService.GetCollection<Faction>("factions");
            
            // 1️⃣ ELIMINAR FACCIONES EXISTENTES
            await factionsCollection.DeleteManyAsync(_ => true);
            Console.WriteLine("✅ Facciones antiguas eliminadas");
            
            // 2️⃣ CREAR LAS 4 FACCIONES CORRECTAS
            var factions = new List<Faction>
            {
                // FACCIÓN 1: BIBLIOTECA
                new Faction
                {
                    Id = "671000000000000000000004", // Nuevo ID
                    Name = "Biblioteca",
                    Type = "Conservadores",
                    Leader = "Los Archiveros",
                    Description = "Guardianes del conocimiento ancestral. Protegen y preservan los datos históricos para que las generaciones futuras puedan aprender del pasado.",
                    ThemeColor = "#8B4513" // Marrón biblioteca
                },
                
                // FACCIÓN 2: GOBIERNO (antes Aletheia)
                new Faction
                {
                    Id = "671000000000000000000001",
                    Name = "Gobierno",
                    Type = "Autoritario",
                    Leader = "El Archivero — Julián Casablancas",
                    Description = "Controlan y regulan el flujo de información. Creen en el orden y la seguridad digital por encima de la libertad individual.",
                    ThemeColor = "#0080FF" // Azul gobierno
                },
                
                // FACCIÓN 3: REDLINE (sin cambios)
                new Faction
                {
                    Id = "671000000000000000000002",
                    Name = "Redline",
                    Type = "Corporativo",
                    Leader = "Decano Villanueva",
                    Description = "La información es poder, y el poder tiene un precio. Controlan los datos para maximizar beneficios y dominar el mercado digital.",
                    ThemeColor = "#FF0080" // Rosa corporativo
                },
                
                // FACCIÓN 4: NEUTRAL (sin cambios)
                new Faction
                {
                    Id = "671000000000000000000003",
                    Name = "Neutral",
                    Type = "Independiente",
                    Leader = "Sin líder",
                    Description = "No se alinean con ninguna facción. Forjan su propio camino sin ataduras ideológicas, tomando decisiones basadas en sus propios principios.",
                    ThemeColor = "#CCCCCC" // Gris neutral
                }
            };
            
            // 3️⃣ INSERTAR NUEVAS FACCIONES
            await factionsCollection.InsertManyAsync(factions);
            Console.WriteLine($"✅ {factions.Count} facciones creadas correctamente\n");
            
            foreach (var faction in factions)
            {
                Console.WriteLine($"   📌 {faction.Name} - {faction.Leader}");
            }
            
            Console.WriteLine("\n✅ Actualización de facciones completada\n");
        }
    }
}