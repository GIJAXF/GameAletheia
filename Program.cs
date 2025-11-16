using Avalonia;
using System;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.ReactiveUI;
using GameAletheiaCross.Services;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;
using GameAletheiaCross.Data;

namespace GameAletheiaCross
{
    public class Program
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("🎮 Iniciando Game Aletheia Cross...");
                
                // 🔗 Inicializa conexión MongoDB
                var dbService = new MongoDbService("mongodb://localhost:27017", "HackerFantasmaDB");

                // ✅ VERIFICAR CONEXIÓN ANTES DE CONTINUAR
                if (!dbService.Ping())
                {
                    Console.WriteLine("❌ ERROR: No se pudo conectar a MongoDB.");
                    Console.WriteLine("⚠️  Asegúrate de que el servicio esté corriendo.");
                    Console.WriteLine("💡 Inicia MongoDB con: mongod");
                    return;
                }

                Console.WriteLine("✅ Conexión a MongoDB establecida");

                // 🆕 ACTUALIZAR FACCIONES (ejecutar solo una vez o cuando necesites actualizar)
                await UpdateFactionsScript.UpdateFactionsAsync();

                var levelRepo = new LevelRepository(dbService);
                var puzzleRepo = new PuzzleRepository(dbService);

                // 🏗️ Genera niveles si no existen
                var generator = new LevelGenerator(levelRepo, puzzleRepo);
                await generator.GenerateDefaultLevelsAsync();

                // 🧩 Genera puzzles avanzados
                Console.WriteLine("🔍 Verificando puzzles de programación...");
                var advancedSeed = new AdvancedSeedData(dbService);
                await advancedSeed.SeedAdvancedPuzzlesAsync();

                // ⚛️ Configura ReactiveUI para Avalonia
                RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

                Console.WriteLine("🖥️  Iniciando interfaz gráfica...\n");

                // 🚀 Inicia la app
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fatal al iniciar: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        // Configuración estándar de Avalonia
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}