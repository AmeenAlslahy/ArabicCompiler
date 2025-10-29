using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace ArabicCompilerIDE
{
    public class RealCodeExecutor
    {
        private readonly string _workingDirectory;
        private readonly string _tempPath;

        public RealCodeExecutor()
        {
            _tempPath = Path.GetTempPath();
            _workingDirectory = Path.Combine(_tempPath, "ArabicCompiler", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_workingDirectory);
        }

        public async Task<ExecutionResult> ExecuteAsync(IntermediateCodeGenerator intermediateCode, string userInput = "")
        {
            try
            {
                // 1. توليد كود C# من الكود الوسيط
                string csharpCode = GenerateCSharpCode(intermediateCode);
                
                // 2. حفظ كود C# في ملف
                string csFilePath = Path.Combine(_workingDirectory, "Program.cs");
                await File.WriteAllTextAsync(csFilePath, csharpCode, Encoding.UTF8);

                // 3. تجميع كود C#
                var compileResult = await CompileCSharpCode(csFilePath);
                if (!compileResult.Success)
                {
                    return new ExecutionResult
                    {
                        Success = false,
                        Output = $"❌ خطأ في التجميع:\n{compileResult.Output}",
                        ExitCode = compileResult.ExitCode
                    };
                }

                // 4. تنفيذ البرنامج المجمع
                var exePath = Path.Combine(_workingDirectory, "Program.exe");
                return await ExecuteCompiledProgram(exePath, userInput);
            }
            catch (Exception ex)
            {
                return new ExecutionResult
                {
                    Success = false,
                    Output = $"❌ خطأ في التنفيذ: {ex.Message}",
                    ExitCode = -1
                };
            }
        }

        private string GenerateCSharpCode(IntermediateCodeGenerator intermediateCode)
        {
            var sb = new StringBuilder();
            
            // رأس برنامج C#
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace ArabicCompilerRuntime");
            sb.AppendLine("{");
            sb.AppendLine("    class Program");
            sb.AppendLine("    {");
            
            // إعلان المتغيرات
            sb.AppendLine("        // المتغيرات المعرفة");
            var variables = ExtractVariables(intermediateCode);
            foreach (var variable in variables)
            {
                sb.AppendLine($"        static int {variable} = 0;");
            }
            
            sb.AppendLine();
            sb.AppendLine("        static void Main(string[] args)");
            sb.AppendLine("        {");
            sb.AppendLine("            // بداية تنفيذ البرنامج");
            sb.AppendLine("            try");
            sb.AppendLine("            {");

            // تحويل التعليمات الوسيطة إلى C#
            foreach (var instruction in intermediateCode.Instructions)
            {
                sb.AppendLine($"                // {instruction.Op}: {instruction}");
                switch (instruction.Op)
                {
                    case OpCode.ASSIGN:
                        sb.AppendLine($"                {instruction.Result} = {GetOperandValue(instruction.Arg1)};");
                        break;
                        
                    case OpCode.ADD:
                        sb.AppendLine($"                {instruction.Result} = {GetOperandValue(instruction.Arg1)} + {GetOperandValue(instruction.Arg2)};");
                        break;
                        
                    case OpCode.SUB:
                        sb.AppendLine($"                {instruction.Result} = {GetOperandValue(instruction.Arg1)} - {GetOperandValue(instruction.Arg2)};");
                        break;
                        
                    case OpCode.MUL:
                        sb.AppendLine($"                {instruction.Result} = {GetOperandValue(instruction.Arg1)} * {GetOperandValue(instruction.Arg2)};");
                        break;
                        
                    case OpCode.DIV:
                        sb.AppendLine($"                {instruction.Result} = {GetOperandValue(instruction.Arg1)} / {GetOperandValue(instruction.Arg2)};");
                        break;
                        
                    case OpCode.READ:
                        sb.AppendLine($"                Console.Write($\"أدخل قيمة {instruction.Result}: \");");
                        sb.AppendLine($"                {instruction.Result} = int.Parse(Console.ReadLine());");
                        break;
                        
                    case OpCode.PRINT:
                        if (instruction.Arg1.Value is string strValue)
                        {
                            sb.AppendLine($"                Console.Write(\"{strValue}\");");
                        }
                        else
                        {
                            sb.AppendLine($"                Console.Write({GetOperandValue(instruction.Arg1)});");
                        }
                        break;
                        
                    case OpCode.HALT:
                        sb.AppendLine("                // نهاية البرنامج");
                        break;
                }
            }

            // نهاية البرنامج
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                Console.WriteLine($\"خطأ أثناء التنفيذ: {ex.Message}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private HashSet<string> ExtractVariables(IntermediateCodeGenerator intermediateCode)
        {
            var variables = new HashSet<string>();
            
            foreach (var instruction in intermediateCode.Instructions)
            {
                if (instruction.Result != null && !instruction.Result.IsTemporary && instruction.Result.Value == null)
                    variables.Add(instruction.Result.Name);
                    
                if (instruction.Arg1 != null && !instruction.Arg1.IsTemporary && instruction.Arg1.Value == null)
                    variables.Add(instruction.Arg1.Name);
                    
                if (instruction.Arg2 != null && !instruction.Arg2.IsTemporary && instruction.Arg2.Value == null)
                    variables.Add(instruction.Arg2.Name);
            }
            
            return variables;
        }

        private string GetOperandValue(Operand operand)
        {
            if (operand == null) return "0";
            if (operand.Value != null) return operand.Value.ToString();
            return operand.Name;
        }

        private async Task<CompilationResult> CompileCSharpCode(string csFilePath)
        {
            var outputPath = Path.Combine(_workingDirectory, "Program.exe");
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{csFilePath}\" --output \"{_workingDirectory}\" --configuration Release --nologo",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using var process = Process.Start(processInfo);
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                return new CompilationResult
                {
                    Success = process.ExitCode == 0,
                    Output = output + error,
                    ExitCode = process.ExitCode
                };
            }
            catch (Exception ex)
            {
                return new CompilationResult
                {
                    Success = false,
                    Output = $"لا يمكن العثور على dotnet SDK: {ex.Message}",
                    ExitCode = -1
                };
            }
        }

        private async Task<ExecutionResult> ExecuteCompiledProgram(string exePath, string userInput)
        {
            if (!File.Exists(exePath))
            {
                return new ExecutionResult
                {
                    Success = false,
                    Output = "❌ لم يتم إنشاء الملف التنفيذي",
                    ExitCode = -1
                };
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{exePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            try
            {
                using var process = Process.Start(processInfo);
                
                // إدخال البيانات إذا كانت موجودة
                if (!string.IsNullOrEmpty(userInput))
                {
                    await process.StandardInput.WriteLineAsync(userInput);
                    process.StandardInput.Close();
                }

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                var result = new ExecutionResult
                {
                    Success = process.ExitCode == 0,
                    Output = output + (string.IsNullOrEmpty(error) ? "" : $"\nالأخطاء: {error}"),
                    ExitCode = process.ExitCode
                };

                // تنظيف الملفات المؤقتة
                Cleanup();
                
                return result;
            }
            catch (Exception ex)
            {
                return new ExecutionResult
                {
                    Success = false,
                    Output = $"❌ خطأ في التنفيذ: {ex.Message}",
                    ExitCode = -1
                };
            }
        }

        private void Cleanup()
        {
            try
            {
                if (Directory.Exists(_workingDirectory))
                {
                    Directory.Delete(_workingDirectory, true);
                }
            }
            catch
            {
                // تجاهل أخطاء التنظيف
            }
        }
    }

    public class ExecutionResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public int ExitCode { get; set; }
    }

    public class CompilationResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public int ExitCode { get; set; }
    }
}