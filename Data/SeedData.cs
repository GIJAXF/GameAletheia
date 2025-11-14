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
            Console.WriteLine("  Generando puzzles de programación avanzados...");

            var levels = await _levelRepo.GetAllAsync();
            
            foreach (var level in levels)
            {
                await CreatePuzzlesForLevel(level);
            }

            Console.WriteLine(" Puzzles avanzados generados");
        }

        private async Task CreatePuzzlesForLevel(Level level)
        {
            var existingPuzzles = await _puzzleRepo.GetAllByLevelIdAsync(level.Id);
            if (existingPuzzles.Count > 0)
            {
                Console.WriteLine($" ️ Puzzles ya existen para nivel {level.OrderNumber}");
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
                Console.WriteLine($"    Puzzle creado: {puzzle.Name}");
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
                },
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Clasificador de Edad",
                    Type = "Condicionales Múltiples",
                    Description = "Dada la variable edad = 25, imprime 'MENOR' si es menor de 18, 'ADULTO' si está entre 18 y 65, 'SENIOR' si es mayor de 65.",
                    ExpectedOutput = "ADULTO",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        int edad = 25;
        // Clasifica la edad
        
    }
}",
                    Difficulty = 2,
                    Hints = new List<string>
                    {
                        "Usa if-else if-else para múltiples condiciones",
                        "Primera condición: edad < 18",
                        "Segunda condición: edad >= 18 && edad <= 65"
                    },
                    Points = 120
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
                    Name = "Conexión de Nodos Digitales",
                    Type = "Bucles",
                    Description = "El sistema de nodos necesita conectarse en secuencia. Usa un bucle for para imprimir los números del 1 al 5, cada uno en una línea.",
                    ExpectedOutput = "1\n2\n3\n4\n5",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        // Conecta los nodos del 1 al 5
        
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
                },
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Suma de Energía del Laberinto",
                    Type = "Bucles con Acumulador",
                    Description = "Los nodos del laberinto necesitan calcular su energía total. Suma los números del 1 al 10 usando un bucle. Imprime solo el resultado final.",
                    ExpectedOutput = "55",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        // Calcula la suma del 1 al 10
        
    }
}",
                    Difficulty = 2,
                    Hints = new List<string>
                    {
                        "Crea una variable suma = 0 antes del bucle",
                        "Dentro del bucle: suma += i",
                        "Imprime suma después del bucle"
                    },
                    Points = 150
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
                },
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Potencia Recursiva",
                    Type = "Recursión Avanzada",
                    Description = "Calcula 2 elevado a la 8 usando recursión. Imprime el resultado.",
                    ExpectedOutput = "256",
                    StarterCode = @"public class Main {
    public static int potencia(int base, int exponente) {
        // Implementa la recursión
        
    }
    
    public static void main(String[] args) {
        System.out.println(potencia(2, 8));
    }
}",
                    Difficulty = 3,
                    Hints = new List<string>
                    {
                        "Caso base: si exponente == 0, retorna 1",
                        "Caso recursivo: return base * potencia(base, exponente - 1)",
                        "2^8 = 256"
                    },
                    Points = 225
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
                },
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Inversor de Array",
                    Type = "Manipulación de Arrays",
                    Description = "Dado el array {1, 2, 3, 4, 5}, inviértelo e imprime los elementos separados por espacios.",
                    ExpectedOutput = "5 4 3 2 1",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        int[] arr = {1, 2, 3, 4, 5};
        // Invierte el array
        
    }
}",
                    Difficulty = 3,
                    Hints = new List<string>
                    {
                        "Recorre el array desde el final: for(int i = arr.length - 1; i >= 0; i--)",
                        "Usa System.out.print(arr[i] + \" \") para la misma línea",
                        "Recuerda el espacio entre números"
                    },
                    Points = 200
                },
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Búsqueda en Array",
                    Type = "Búsqueda",
                    Description = "Dado el array {10, 20, 30, 40, 50}, busca el número 30 e imprime su índice (posición). Si no existe, imprime -1.",
                    ExpectedOutput = "2",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        int[] arr = {10, 20, 30, 40, 50};
        int buscar = 30;
        // Encuentra el índice
        
    }
}",
                    Difficulty = 3,
                    Hints = new List<string>
                    {
                        "Recorre el array con un bucle for",
                        "Si arr[i] == buscar, imprime i y usa break",
                        "El índice de 30 es 2 (recuerda que los índices empiezan en 0)"
                    },
                    Points = 220
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
                    Name = "Número Primo",
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
                    Points = 280
                },
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Palíndromo Numérico",
                    Type = "Algoritmos de Cadenas",
                    Description = "Verifica si el número 12321 es un palíndromo (se lee igual al revés). Imprime 'SI' o 'NO'.",
                    ExpectedOutput = "SI",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        int numero = 12321;
        // Verifica si es palíndromo
        
    }
}",
                    Difficulty = 4,
                    Hints = new List<string>
                    {
                        "Convierte el número a String: String str = String.valueOf(numero)",
                        "Invierte el String y compáralo con el original",
                        "Puedes usar StringBuilder para invertir: new StringBuilder(str).reverse().toString()"
                    },
                    Points = 300
                },
                new Puzzle
                {
                    LevelId = levelId,
                    Name = "Ordenamiento Burbuja",
                    Type = "Algoritmos de Ordenamiento",
                    Description = "Ordena el array {5, 2, 8, 1, 9} usando el algoritmo de burbuja. Imprime los elementos ordenados separados por espacios.",
                    ExpectedOutput = "1 2 5 8 9",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        int[] arr = {5, 2, 8, 1, 9};
        // Ordena usando burbuja
        
    }
}",
                    Difficulty = 5,
                    Hints = new List<string>
                    {
                        "Usa dos bucles anidados",
                        "En cada iteración, compara elementos adyacentes",
                        "Si arr[j] > arr[j+1], intercámbialos usando una variable temporal"
                    },
                    Points = 350
                }
            };
        }
    }
}