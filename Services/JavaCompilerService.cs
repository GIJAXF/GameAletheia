using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameAletheiaCross.Services
{
    public class JavaCompilerService
    {
        private string _tempDirectory;
        
        public JavaCompilerService()
        {
            CreateNewTempDirectory();
        }
        
        private void CreateNewTempDirectory()
        {
            // Crear un directorio temporal único para cada sesión
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"HackerFantasma_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);
            Console.WriteLine($"  Directorio temporal creado: {_tempDirectory}");
        }
        
        public async Task<CompilationResult> CompileAndRunAsync(string code, string expectedOutput)
        {
            var result = new CompilationResult();
            
            try
            {
                //   IMPORTANTE: Limpiar archivos anteriores ANTES de compilar
                CleanupTempFiles();
                
                // Recrear el directorio temporal
                if (!Directory.Exists(_tempDirectory))
                {
                    Directory.CreateDirectory(_tempDirectory);
                }
                
                string className = ExtractClassName(code);
                if (string.IsNullOrEmpty(className))
                {
                    result.Success = false;
                    result.ErrorMessage = "No se encontró una clase pública en el código";
                    return result;
                }
                
                string sourceFile = Path.Combine(_tempDirectory, $"{className}.java");
                
                // Escribir el código en el archivo
                await File.WriteAllTextAsync(sourceFile, code);
                Console.WriteLine($"  Código escrito en: {sourceFile}");
                
                // Compilar
                var compileResult = await CompileAsync(sourceFile, className);
                if (!compileResult.Success)
                {
                    Console.WriteLine($"  Compilación falló");
                    return compileResult;
                }
                
                Console.WriteLine($"  Compilación exitosa");
                
                // Ejecutar
                var executeResult = await ExecuteAsync(className);
                if (!executeResult.Success)
                {
                    Console.WriteLine($"  Ejecución falló");
                    return executeResult;
                }
                
                Console.WriteLine($"  Ejecución exitosa");
                
                // Comparar salida
                result.Output = executeResult.Output;
                result.Success = CompareOutput(result.Output, expectedOutput);
                
                if (!result.Success)
                {
                    result.ErrorMessage = $"Salida incorrecta.\nEsperado: '{expectedOutput}'\nObtenido: '{result.Output}'";
                }
                else
                {
                    Console.WriteLine($"  Salida correcta");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error: {ex.Message}";
                Console.WriteLine($"  Error en CompileAndRunAsync: {ex.Message}");
            }
            finally
            {
                // Limpiar después de ejecutar
                CleanupTempFiles();
            }
            
            return result;
        }
        
        private string ExtractClassName(string code)
        {
            var match = Regex.Match(code, @"public\s+class\s+(\w+)");
            return match.Success ? match.Groups[1].Value : null;
        }
        
        private async Task<CompilationResult> CompileAsync(string sourceFile, string className)
        {
            var result = new CompilationResult();
            
            try
            {
                // Eliminar archivos .class antiguos antes de compilar
                var oldClassFiles = Directory.GetFiles(_tempDirectory, "*.class");
                foreach (var file in oldClassFiles)
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"  Eliminado: {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  No se pudo eliminar {file}: {ex.Message}");
                    }
                }
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "javac",
                    Arguments = $"\"{sourceFile}\"",
                    WorkingDirectory = _tempDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "No se pudo iniciar javac. ¿Está instalado JDK?";
                        return result;
                    }
                    
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode != 0)
                    {
                        result.Success = false;
                        result.ErrorMessage = $"Error de compilación:\n{error}";
                        Console.WriteLine($"  javac ExitCode: {process.ExitCode}");
                        Console.WriteLine($"   Error: {error}");
                    }
                    else
                    {
                        result.Success = true;
                        
                        // Verificar que el archivo .class se creó
                        string classFile = Path.Combine(_tempDirectory, $"{className}.class");
                        if (File.Exists(classFile))
                        {
                            Console.WriteLine($"  Archivo .class creado: {classFile}");
                        }
                        else
                        {
                            Console.WriteLine($"  Advertencia: archivo .class no encontrado");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error ejecutando javac: {ex.Message}";
                Console.WriteLine($"  Excepción en CompileAsync: {ex.Message}");
            }
            
            return result;
        }
        
        private async Task<CompilationResult> ExecuteAsync(string className)
        {
            var result = new CompilationResult();
            
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = className,
                    WorkingDirectory = _tempDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "No se pudo iniciar java";
                        return result;
                    }
                    
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    
                    await process.WaitForExitAsync();
                    
                    if (!string.IsNullOrEmpty(error))
                    {
                        result.Success = false;
                        result.ErrorMessage = $"Error de ejecución:\n{error}";
                        Console.WriteLine($"  java error: {error}");
                    }
                    else
                    {
                        result.Success = true;
                        result.Output = output.Trim();
                        Console.WriteLine($"  Output: '{result.Output}'");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error ejecutando java: {ex.Message}";
                Console.WriteLine($"  Excepción en ExecuteAsync: {ex.Message}");
            }
            
            return result;
        }
        
        private bool CompareOutput(string actual, string expected)
        {
            actual = actual?.Trim().Replace("\r\n", "\n") ?? "";
            expected = expected?.Trim().Replace("\r\n", "\n") ?? "";
            
            bool matches = actual.Equals(expected, StringComparison.Ordinal);
            
            Console.WriteLine($"  Comparación de salida:");
            Console.WriteLine($"   Esperado: '{expected}'");
            Console.WriteLine($"   Obtenido: '{actual}'");
            Console.WriteLine($"   Coincide: {matches}");
            
            return matches;
        }
        
        private void CleanupTempFiles()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    // Intentar eliminar todos los archivos
                    var files = Directory.GetFiles(_tempDirectory);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  No se pudo eliminar {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                    
                    // Si el directorio está vacío, eliminarlo
                    try
                    {
                        if (Directory.GetFiles(_tempDirectory).Length == 0)
                        {
                            Directory.Delete(_tempDirectory, false);
                            Console.WriteLine($"  Directorio temporal eliminado");
                            
                            // Crear uno nuevo para la próxima compilación
                            CreateNewTempDirectory();
                        }
                    }
                    catch
                    {
                        // Ignorar si no se puede eliminar el directorio
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error limpiando archivos temporales: {ex.Message}");
            }
        }
        
        // Método para limpiar al finalizar la aplicación
        public void Dispose()
        {
            CleanupTempFiles();
        }
    }
    
    public class CompilationResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }
}