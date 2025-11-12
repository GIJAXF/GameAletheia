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
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"HackerFantasma_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);
        }
        
        public async Task<CompilationResult> CompileAndRunAsync(string code, string expectedOutput)
        {
            var result = new CompilationResult();
            
            try
            {
                string className = ExtractClassName(code);
                if (string.IsNullOrEmpty(className))
                {
                    result.Success = false;
                    result.ErrorMessage = "No se encontró una clase pública en el código";
                    return result;
                }
                
                string sourceFile = Path.Combine(_tempDirectory, $"{className}.java");
                await File.WriteAllTextAsync(sourceFile, code);
                
                var compileResult = await CompileAsync(sourceFile);
                if (!compileResult.Success) return compileResult;
                
                var executeResult = await ExecuteAsync(className);
                if (!executeResult.Success) return executeResult;
                
                result.Output = executeResult.Output;
                result.Success = CompareOutput(result.Output, expectedOutput);
                if (!result.Success)
                {
                    result.ErrorMessage = $"Salida incorrecta.\nEsperado: '{expectedOutput}'\nObtenido: '{result.Output}'";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                CleanupTempFiles();
            }
            
            return result;
        }
        
        private string ExtractClassName(string code)
        {
            var match = Regex.Match(code, @"public\s+class\s+(\w+)");
            return match.Success ? match.Groups[1].Value : null;
        }
        
        private async Task<CompilationResult> CompileAsync(string sourceFile)
        {
            var result = new CompilationResult();
            
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
                }
                else
                {
                    result.Success = true;
                }
            }
            
            return result;
        }
        
        private async Task<CompilationResult> ExecuteAsync(string className)
        {
            var result = new CompilationResult();
            
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
                }
                else
                {
                    result.Success = true;
                    result.Output = output.Trim();
                }
            }
            
            return result;
        }
        
        private bool CompareOutput(string actual, string expected)
        {
            actual = actual?.Trim().Replace("\r\n", "\n") ?? "";
            expected = expected?.Trim().Replace("\r\n", "\n") ?? "";
            return actual.Equals(expected, StringComparison.Ordinal);
        }
        
        private void CleanupTempFiles()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error limpiando archivos: {ex.Message}");
            }
        }
    }
    
    public class CompilationResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }
}