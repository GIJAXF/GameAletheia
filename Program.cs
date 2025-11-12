using Avalonia;
using System;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.ReactiveUI;
using GameAletheiaCross.Services;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;

namespace GameAletheiaCross
{
    public class Program
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            try
            {
                // 🧠 Inicializa conexión MongoDB
                var dbService = new MongoDbService("mongodb://localhost:27017", "HackerFantasmaDB");
                var levelRepo = new LevelRepository(dbService);
                var puzzleRepo = new PuzzleRepository(dbService);

                // 🧩 Genera niveles si no existen
                var generator = new LevelGenerator(levelRepo, puzzleRepo);
                await generator.GenerateDefaultLevelsAsync();

                // ⚙️ Configura ReactiveUI para Avalonia
                RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

                // 🚀 Inicia la app
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💀 Error fatal al iniciar: {ex.Message}");
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
