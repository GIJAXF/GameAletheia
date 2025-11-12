using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;
using MongoDB.Driver;

namespace GameAletheiaCross.Data
{
    public class AdvancedSeedData
    {
        private readonly MongoDbService _dbService;
        private readonly PuzzleRepository _puzzleRepo;
        private readonly LevelRepository _levelRepo;

        public AdvancedSeedData(MongoDbService dbService)
        {
            _dbService = dbService;
            _puzzleRepo = new PuzzleRepository(dbService);
            _levelRepo = new LevelRepository(dbService);
        }

        public async Task SeedAdvancedPuzzlesAsync()
        {
            Console.WriteLine("🧩 Generando puzzles de programación avanzados...");

            var levels = await _levelRepo.GetAllAsync();
            
            foreach (var level in levels)
            {
                await CreatePuzzlesForLevel(level);
            }

            Console.WriteLine("✅ Puzzles avanzados generados");
        }

        private async Task CreatePuzzlesForLevel(Level level)
        {
            var existingPuzzles = await _puzzleRepo.GetAllByLevelIdAsync(level.Id);
            if (existingPuzzles.Count > 0)
            {
                Console.WriteLine($"⚠️ Puzzles ya existen para nivel {level.OrderNumber}");
                return;
            }

            List<Puzzle> puzzles = level.OrderNumber switch
            {
                1 => CreateLevel1Puzzles(level.Id),
                2 => CreateLevel2Puzzles(level.Id),
                3 => CreateLevel3Puzzles(level.Id),
                4 => CreateLevel4Puzzles(level.Id),
                5 => CreateLevel5Puzzles(level.Id),
                6 => CreateLevel6Puzzles(level.Id),
                7 => CreateLevel7Puzzles(level.Id),
                _ => new List<Puzzle>()
            };

            foreach (var puzzle in puzzles)
            {
                await _puzzleRepo.CreateAsync(puzzle);
                Console.WriteLine($"  ✓ Puzzle creado: {puzzle.Name}");
            }
        }

        private List<Puzzle> CreateLevel1Puzzles(string levelId)
        {
            return new List<Puzzle>
            {
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Bienvenida Digital",
                    Type = "Tutorial",
                    Description = "Tu primera misión: Imprime 'Bienvenido a Aletheia' en la consola.",
                    ExpectedOutput = "Bienvenido a Aletheia",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        // Escribe tu código aquí
        
    }
}",
                    Difficulty = 1,
                    Hints = new List<string>
                    {
                        "Usa System.out.println() para imprimir en consola",
                        "El texto debe ir entre comillas dobles: \"Bienvenido a Aletheia\"",
                        "No olvides el punto y coma (;) al final"
                    },
                    Points = 50
                }
            };
        }

        private List<Puzzle> CreateLevel2Puzzles(string levelId)
        {
            return new List<Puzzle>
            {
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Suma de Dos Números",
                    Type = "Aritmética",
                    Description = "Crea dos variables enteras con los valores 15 y 27, súmalas e imprime el resultado.",
                    ExpectedOutput = "42",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        // Define dos variables y súmalas
        
    }
}",
                    Difficulty = 1,
                    Hints = new List<string>
                    {
                        "Usa 'int' para declarar variables enteras",
                        "Ejemplo: int a = 15;",
                        "Suma las variables y guarda en una tercera: int suma = a + b;"
                    },
                    Points = 75
                }
            };
        }

        private List<Puzzle> CreateLevel3Puzzles(string levelId)
        {
            return new List<Puzzle>
            {
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Detector de Pares",
                    Type = "Condicionales",
                    Description = "Crea una variable con el valor 8. Si es par, imprime 'PAR', si es impar imprime 'IMPAR'.",
                    ExpectedOutput = "PAR",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        int numero = 8;
        // Verifica si es par o impar
        
    }
}",
                    Difficulty = 2,
                    Hints = new List<string>
                    {
                        "Usa el operador módulo (%) para verificar si es par",
                        "Si numero % 2 == 0, entonces es par",
                        "Usa if-else para la condición"
                    },
                    Points = 100
                }
            };
        }

        private List<Puzzle> CreateLevel4Puzzles(string levelId)
        {
            return new List<Puzzle>
            {
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Bucle de Energía",
                    Type = "Bucles",
                    Description = "Usa un bucle for para imprimir los números del 1 al 5, cada uno en una línea.",
                    ExpectedOutput = "1\n2\n3\n4\n5",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        // Usa un bucle for
        
    }
}",
                    Difficulty = 2,
                    Hints = new List<string>
                    {
                        "Sintaxis: for(int i = 1; i <= 5; i++)",
                        "Dentro del bucle usa System.out.println(i)",
                        "Cada número debe estar en una línea separada"
                    },
                    Points = 125
                }
            };
        }

        private List<Puzzle> CreateLevel5Puzzles(string levelId)
        {
            return new List<Puzzle>
            {
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Factorial Recursivo",
                    Type = "Recursión",
                    Description = "Calcula el factorial de 5 usando una función recursiva. Imprime solo el resultado.",
                    ExpectedOutput = "120",
                    StarterCode = @"public class Main {
    public static int factorial(int n) {
        // Implementa la recursión aquí
        
    }
    
    public static void main(String[] args) {
        System.out.println(factorial(5));
    }
}",
                    Difficulty = 3,
                    Hints = new List<string>
                    {
                        "Caso base: si n == 0 o n == 1, retorna 1",
                        "Caso recursivo: return n * factorial(n - 1)",
                        "El factorial de 5 es 5 * 4 * 3 * 2 * 1 = 120"
                    },
                    Points = 200
                }
            };
        }

        private List<Puzzle> CreateLevel6Puzzles(string levelId)
        {
            return new List<Puzzle>
            {
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Array de Datos Corporativos",
                    Type = "Arrays",
                    Description = "Crea un array con los números {3, 7, 1, 9, 2}, encuentra el máximo e imprímelo.",
                    ExpectedOutput = "9",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        int[] datos = {3, 7, 1, 9, 2};
        // Encuentra el máximo
        
    }
}",
                    Difficulty = 3,
                    Hints = new List<string>
                    {
                        "Crea una variable 'max' con el primer elemento",
                        "Recorre el array con un bucle for",
                        "Si datos[i] > max, actualiza max = datos[i]"
                    },
                    Points = 175
                }
            };
        }

        private List<Puzzle> CreateLevel7Puzzles(string levelId)
        {
            return new List<Puzzle>
            {
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Secuencia de Fibonacci",
                    Type = "Algoritmos",
                    Description = "Imprime los primeros 7 números de la secuencia de Fibonacci separados por espacios.",
                    ExpectedOutput = "0 1 1 2 3 5 8",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        // Genera la secuencia de Fibonacci
        
    }
}",
                    Difficulty = 4,
                    Hints = new List<string>
                    {
                        "Inicia con a = 0, b = 1",
                        "En cada iteración: siguiente = a + b, luego a = b, b = siguiente",
                        "Usa System.out.print() en lugar de println para la misma línea"
                    },
                    Points = 250
                },
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Desafío Final: Número Primo",
                    Type = "Algoritmos Avanzados",
                    Description = "Verifica si el número 17 es primo. Imprime 'PRIMO' si lo es, 'NO PRIMO' si no.",
                    ExpectedOutput = "PRIMO",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        int numero = 17;
        // Verifica si es primo
        
    }
}",
                    Difficulty = 4,
                    Hints = new List<string>
                    {
                        "Un número es primo si solo es divisible por 1 y por sí mismo",
                        "Usa un bucle para verificar divisibilidad desde 2 hasta numero/2",
                        "Si encuentras un divisor, no es primo"
                    },
                    Points = 300
                }
            };
        }
    }
}