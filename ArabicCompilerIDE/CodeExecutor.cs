using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ArabicCompilerIDE
{
    public class CodeExecutor
    {
        private readonly string _assemblyCode;
        private readonly string _workingDirectory;

        public CodeExecutor(string assemblyCode, string workingDirectory)
        {
            _assemblyCode = assemblyCode;
            _workingDirectory = workingDirectory;
        }

        public async Task<ExecutionResult> ExecuteAsync(string input = "")
        {
            try
            {
                // 1. حفظ كود التجميع في ملف
                string asmFile = Path.Combine(_workingDirectory, "program.asm");
                string objFile = Path.Combine(_workingDirectory, "program.o");
                string exeFile = Path.Combine(_workingDirectory, "program.exe");

                await File.WriteAllTextAsync(asmFile, _assemblyCode);

                // 2. تجميع باستخدام NASM (يجب تثبيته)
                var assembleProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "nasm",
                        Arguments = $"-f win32 {asmFile} -o {objFile}",
                        WorkingDirectory = _workingDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                assembleProcess.Start();
                string assembleOutput = await assembleProcess.StandardOutput.ReadToEndAsync();
                string assembleError = await assembleProcess.StandardError.ReadToEndAsync();
                await assembleProcess.WaitForExitAsync();

                if (assembleProcess.ExitCode != 0)
                {
                    return new ExecutionResult
                    {
                        Success = false,
                        Output = $"خطأ في التجميع: {assembleError}",
                        ExitCode = assembleProcess.ExitCode
                    };
                }

                // 3. ربط باستخدام linker (يجب تثبيت MinGW أو VS Build Tools)
                var linkProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "gcc",
                        Arguments = $"{objFile} -o {exeFile}",
                        WorkingDirectory = _workingDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                linkProcess.Start();
                string linkOutput = await linkProcess.StandardOutput.ReadToEndAsync();
                string linkError = await linkProcess.StandardError.ReadToEndAsync();
                await linkProcess.WaitForExitAsync();

                if (linkProcess.ExitCode != 0)
                {
                    return new ExecutionResult
                    {
                        Success = false,
                        Output = $"خطأ في الربط: {linkError}",
                        ExitCode = linkProcess.ExitCode
                    };
                }

                // 4. تنفيذ البرنامج
                var executeProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exeFile,
                        WorkingDirectory = _workingDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true
                    }
                };

                executeProcess.Start();

                // إدخال البيانات إذا كانت مطلوبة
                if (!string.IsNullOrEmpty(input))
                {
                    await executeProcess.StandardInput.WriteLineAsync(input);
                    executeProcess.StandardInput.Close();
                }

                string output = await executeProcess.StandardOutput.ReadToEndAsync();
                string error = await executeProcess.StandardError.ReadToEndAsync();
                await executeProcess.WaitForExitAsync();

                return new ExecutionResult
                {
                    Success = executeProcess.ExitCode == 0,
                    Output = output + (string.IsNullOrEmpty(error) ? "" : $"\nالأخطاء: {error}"),
                    ExitCode = executeProcess.ExitCode
                };
            }
            catch (Exception ex)
            {
                return new ExecutionResult
                {
                    Success = false,
                    Output = $"خطأ في التنفيذ: {ex.Message}",
                    ExitCode = -1
                };
            }
        }
    }

    public class ExecutionResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public int ExitCode { get; set; }
    }
}