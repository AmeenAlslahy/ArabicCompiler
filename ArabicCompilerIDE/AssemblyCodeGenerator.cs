using System;
using System.Collections.Generic;
using System.Text;

namespace ArabicCompiler
{
    // مولد كود التجميع x86
    public class AssemblyCodeGenerator
    {
        private readonly IntermediateCodeGenerator _intermediateCode;
        private readonly StringBuilder _assemblyCode;
        private int _labelCounter;
        private int _tempCounter;

        public AssemblyCodeGenerator(IntermediateCodeGenerator intermediateCode)
        {
            _intermediateCode = intermediateCode;
            _assemblyCode = new StringBuilder();
            _labelCounter = 0;
            _tempCounter = 0;
        }

        public string Generate()
        {
            GenerateHeader();
            GenerateDataSection();
            GenerateCodeSection();
            GenerateFooter();
            
            return _assemblyCode.ToString();
        }

        // قسم البيانات والثوابت
        private void GenerateDataSection()
        {
            _assemblyCode.AppendLine("section .data");
            _assemblyCode.AppendLine();
            
            // إضافة ثوابت السلاسل النصية
            _assemblyCode.AppendLine("    ; السلاسل النصية للطباعة");
            _assemblyCode.AppendLine("    msg_hello db 'مرحبا بالعالم!', 0");
            _assemblyCode.AppendLine("    msg_result db 'النتيجة: ', 0");
            _assemblyCode.AppendLine("    newline db 10, 0");
            _assemblyCode.AppendLine();

            // إضافة متغيرات البرنامج
            _assemblyCode.AppendLine("    ; متغيرات البرنامج");
            _assemblyCode.AppendLine("    x dd 0");
            _assemblyCode.AppendLine("    y dd 0");
            _assemblyCode.AppendLine("    z dd 0");
            _assemblyCode.AppendLine();
        }

        // رأس كود التجميع
        private void GenerateHeader()
        {
            _assemblyCode.AppendLine("; =========================================");
            _assemblyCode.AppendLine("; كود تجميع مولد تلقائياً من مترجم اللغة العربية");
            _assemblyCode.AppendLine("; =========================================");
            _assemblyCode.AppendLine();
            
            _assemblyCode.AppendLine("global _start");
            _assemblyCode.AppendLine("extern printf, scanf, exit");
            _assemblyCode.AppendLine();
        }

        // قسم الكود التنفيذي
        private void GenerateCodeSection()
        {
            _assemblyCode.AppendLine("section .text");
            _assemblyCode.AppendLine();

            _assemblyCode.AppendLine("_start:");
            _assemblyCode.AppendLine("    ; بداية البرنامج");
            _assemblyCode.AppendLine();

            // توليد الكود من التعليمات الوسيطة
            GenerateFromIntermediateCode();

            _assemblyCode.AppendLine("    ; نهاية البرنامج");
            _assemblyCode.AppendLine("    call exit_program");
            _assemblyCode.AppendLine();
        }

        // توليد الكود من التعليمات الوسيطة
        private void GenerateFromIntermediateCode()
        {
            foreach (var instruction in _intermediateCode.Instructions)
            {
                GenerateInstruction(instruction);
            }
        }

        // توليد تعليمة تجميع من تعليمة وسيطة
        private void GenerateInstruction(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"    ; {instruction.Op} -> {instruction}");

            switch (instruction.Op)
            {
                case OpCode.ASSIGN:
                    GenerateAssign(instruction);
                    break;
                case OpCode.ADD:
                    GenerateAdd(instruction);
                    break;
                case OpCode.SUB:
                    GenerateSub(instruction);
                    break;
                case OpCode.MUL:
                    GenerateMul(instruction);
                    break;
                case OpCode.DIV:
                    GenerateDiv(instruction);
                    break;
                case OpCode.PRINT:
                    GeneratePrint(instruction);
                    break;
                case OpCode.READ:
                    GenerateRead(instruction);
                    break;
                case OpCode.HALT:
                    GenerateHalt();
                    break;
                case OpCode.LABEL:
                    GenerateLabel(instruction);
                    break;
                case OpCode.GOTO:
                    GenerateGoto(instruction);
                    break;
                case OpCode.IF_GOTO:
                    GenerateIfGoto(instruction);
                    break;
                default:
                    _assemblyCode.AppendLine($"    ; تعليمة غير مدعومة: {instruction.Op}");
                    break;
            }

            _assemblyCode.AppendLine();
        }

        private void GenerateAssign(IntermediateInstruction instruction)
        {
            if (instruction.Arg1 != null)
            {
                _assemblyCode.AppendLine($"    ; ASSIGN {instruction.Result} = {instruction.Arg1}");
                _assemblyCode.AppendLine($"    mov eax, [{instruction.Arg1}]");
                _assemblyCode.AppendLine($"    mov [{instruction.Result}], eax");
            }
        }

        private void GenerateAdd(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"    ; ADD {instruction.Result} = {instruction.Arg1} + {instruction.Arg2}");
            _assemblyCode.AppendLine($"    mov eax, [{instruction.Arg1}]");
            _assemblyCode.AppendLine($"    add eax, [{instruction.Arg2}]");
            _assemblyCode.AppendLine($"    mov [{instruction.Result}], eax");
        }

        private void GenerateSub(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"    ; SUB {instruction.Result} = {instruction.Arg1} - {instruction.Arg2}");
            _assemblyCode.AppendLine($"    mov eax, [{instruction.Arg1}]");
            _assemblyCode.AppendLine($"    sub eax, [{instruction.Arg2}]");
            _assemblyCode.AppendLine($"    mov [{instruction.Result}], eax");
        }

        private void GenerateMul(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"    ; MUL {instruction.Result} = {instruction.Arg1} * {instruction.Arg2}");
            _assemblyCode.AppendLine($"    mov eax, [{instruction.Arg1}]");
            _assemblyCode.AppendLine($"    imul eax, [{instruction.Arg2}]");
            _assemblyCode.AppendLine($"    mov [{instruction.Result}], eax");
        }

        private void GenerateDiv(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"    ; DIV {instruction.Result} = {instruction.Arg1} / {instruction.Arg2}");
            _assemblyCode.AppendLine($"    mov eax, [{instruction.Arg1}]");
            _assemblyCode.AppendLine($"    cdq");
            _assemblyCode.AppendLine($"    idiv dword [{instruction.Arg2}]");
            _assemblyCode.AppendLine($"    mov [{instruction.Result}], eax");
        }

        private void GeneratePrint(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"    ; PRINT {instruction.Arg1}");
            
            if (instruction.Arg1.Value is string)
            {
                // طباعة سلسلة نصية
                _assemblyCode.AppendLine($"    push msg_{GenerateStringLabel(instruction.Arg1.Value.ToString())}");
            }
            else
            {
                // طباعة عدد
                _assemblyCode.AppendLine($"    push msg_result");
                _assemblyCode.AppendLine($"    call printf");
                _assemblyCode.AppendLine($"    add esp, 4");
                
                _assemblyCode.AppendLine($"    push dword [{instruction.Arg1}]");
            }
            
            _assemblyCode.AppendLine($"    call printf");
            _assemblyCode.AppendLine($"    add esp, 4");
            
            // طباعة سطر جديد
            _assemblyCode.AppendLine($"    push newline");
            _assemblyCode.AppendLine($"    call printf");
            _assemblyCode.AppendLine($"    add esp, 4");
        }

        private void GenerateRead(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"    ; READ {instruction.Result}");
            _assemblyCode.AppendLine($"    push {instruction.Result}");
            _assemblyCode.AppendLine($"    push scan_format");
            _assemblyCode.AppendLine($"    call scanf");
            _assemblyCode.AppendLine($"    add esp, 8");
        }

        private void GenerateHalt()
        {
            _assemblyCode.AppendLine("    ; HALT - نهاية البرنامج");
            _assemblyCode.AppendLine("    mov eax, 1");
            _assemblyCode.AppendLine("    xor ebx, ebx");
            _assemblyCode.AppendLine("    int 0x80");
        }

        private void GenerateLabel(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"{instruction.Result}:");
        }

        private void GenerateGoto(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"    jmp {instruction.Result}");
        }

        private void GenerateIfGoto(IntermediateInstruction instruction)
        {
            _assemblyCode.AppendLine($"    cmp [{instruction.Arg1}], 0");
            _assemblyCode.AppendLine($"    jne {instruction.Result}");
        }

        // دوال مساعدة
        private string GenerateStringLabel(string str)
        {
            return $"str_{Math.Abs(str.GetHashCode())}";
        }

        private string NewLabel()
        {
            return $"L{_labelCounter++}";
        }

        private string NewTemp()
        {
            return $"T{_tempCounter++}";
        }

        // تذييل كود التجميع
        private void GenerateFooter()
        {
            _assemblyCode.AppendLine();
            _assemblyCode.AppendLine("; =========================================");
            _assemblyCode.AppendLine("; دوال مساعدة");
            _assemblyCode.AppendLine("; =========================================");
            _assemblyCode.AppendLine();

            _assemblyCode.AppendLine("exit_program:");
            _assemblyCode.AppendLine("    mov eax, 1");
            _assemblyCode.AppendLine("    xor ebx, ebx");
            _assemblyCode.AppendLine("    int 0x80");
            _assemblyCode.AppendLine();

            _assemblyCode.AppendLine("; تنسيقات الإدخال والإخراج");
            _assemblyCode.AppendLine("scan_format db '%d', 0");
            _assemblyCode.AppendLine("print_format db '%d', 0");
            _assemblyCode.AppendLine("string_format db '%s', 0");
        }
    }
}