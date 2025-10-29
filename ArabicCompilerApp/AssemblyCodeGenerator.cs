using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArabicCompiler
{
    // فئة لتوليد كود التجميع (x86 Assembly)
    public class AssemblyCodeGenerator
    {
        private readonly IntermediateCodeGenerator _icg;
        private readonly StringBuilder _assemblyCode = new StringBuilder();
        private readonly Dictionary<string, string> _variableLocations = new Dictionary<string, string>();
        private int _stackOffset = 0;

        public AssemblyCodeGenerator(IntermediateCodeGenerator icg)
        {
            _icg = icg;
        }

        public string Generate()
        {
            // 1. تهيئة الكود (Data Section)
            GenerateDataSection();

            // 2. تهيئة الكود (Text Section - Main Function)
            GenerateTextSectionStart();

            // 3. ترجمة تعليمات الكود الوسيط
            TranslateIntermediateCode();

            // 4. إنهاء الكود
            GenerateTextSectionEnd();

            return _assemblyCode.ToString();
        }

        private void GenerateDataSection()
        {
            _assemblyCode.AppendLine("section .data");
            _assemblyCode.AppendLine("    ; تعريف المتغيرات والثوابت");
            // يجب هنا تحليل جدول الرموز لتحديد المتغيرات
            // للتبسيط، سنفترض أن جميع المتغيرات هي 4 بايت (DWORD)
            // وسنضيف متغيرات الإدخال/الإخراج القياسية
            _assemblyCode.AppendLine("    msg_newline db 0xA, 0xD");
            _assemblyCode.AppendLine("    len_newline equ $ - msg_newline");
            _assemblyCode.AppendLine();
        }

        private void GenerateTextSectionStart()
        {
            _assemblyCode.AppendLine("section .text");
            _assemblyCode.AppendLine("    global _start");
            _assemblyCode.AppendLine();
            _assemblyCode.AppendLine("_start:");
            _assemblyCode.AppendLine("    ; تهيئة المكدس (Stack)");
            _assemblyCode.AppendLine("    push ebp");
            _assemblyCode.AppendLine("    mov ebp, esp");
            _assemblyCode.AppendLine("    sub esp, 4096 ; حجز مساحة للمتغيرات المحلية (افتراضية)");
            _assemblyCode.AppendLine();
        }

        private void GenerateTextSectionEnd()
        {
            _assemblyCode.AppendLine();
            _assemblyCode.AppendLine("    ; إنهاء البرنامج");
            _assemblyCode.AppendLine("    mov eax, 1    ; sys_exit");
            _assemblyCode.AppendLine("    xor ebx, ebx  ; exit code 0");
            _assemblyCode.AppendLine("    int 0x80");
        }

        private void TranslateIntermediateCode()
        {
            foreach (var instruction in _icg.Instructions)
            {
                _assemblyCode.AppendLine($"; {instruction}"); // تعليق بالكود الوسيط الأصلي
                switch (instruction.Op)
                {
                    case OpCode.ASSIGN:
                        TranslateAssignment(instruction);
                        break;
                    case OpCode.ADD:
                    case OpCode.SUB:
                    case OpCode.MUL:
                        TranslateBinaryOp(instruction);
                        break;
                    case OpCode.READ:
                        TranslateRead(instruction);
                        break;
                    case OpCode.PRINT:
                        TranslatePrint(instruction);
                        break;
                    case OpCode.LABEL:
                        _assemblyCode.AppendLine($"{instruction.Result.Name}:");
                        break;
                    case OpCode.HALT:
                        // سيتم التعامل معها في GenerateTextSectionEnd
                        break;
                    default:
                        _assemblyCode.AppendLine($"; WARNING: OpCode {instruction.Op} not yet implemented.");
                        break;
                }
            }
        }

        // وظيفة مساعدة للحصول على موقع المتغير (المكدس أو الذاكرة)
        private string GetVariableLocation(Operand operand)
        {
            if (operand.IsTemporary || !char.IsLetter(operand.Name.First()))
            {
                // المتغيرات المؤقتة والثوابت يتم التعامل معها عبر المسجلات أو قيم فورية
                return operand.ToString();
            }

            if (!_variableLocations.ContainsKey(operand.Name))
            {
                // تخصيص موقع جديد على المكدس للمتغير غير المعروف
                _stackOffset -= 4; // 4 بايت للمتغير (DWORD)
                _variableLocations[operand.Name] = $"[ebp{_stackOffset}]";
            }
            return _variableLocations[operand.Name];
        }

        private void TranslateAssignment(IntermediateInstruction instruction)
        {
            var dest = GetVariableLocation(instruction.Result);
            var source = instruction.Arg1;

            if (source.IsTemporary || char.IsLetter(source.Name.First()))
            {
                // المصدر هو متغير أو مؤقت
                var sourceLoc = GetVariableLocation(source);
                _assemblyCode.AppendLine($"    mov eax, {sourceLoc}");
                _assemblyCode.AppendLine($"    mov {dest}, eax");
            }
            else
            {
                // المصدر هو ثابت (رقمي فقط حاليًا)
                if (int.TryParse(source.Name, out _))
                {
                    _assemblyCode.AppendLine($"    mov dword {dest}, {source.Name}");
                }
                else
                {
                    _assemblyCode.AppendLine($"; ERROR: Cannot assign non-integer constant {source.Name}");
                }
            }
        }

        private void TranslateBinaryOp(IntermediateInstruction instruction)
        {
            var result = GetVariableLocation(instruction.Result);
            var arg1 = GetVariableLocation(instruction.Arg1);
            var arg2 = GetVariableLocation(instruction.Arg2);

            // 1. تحميل المعامل الأول
            _assemblyCode.AppendLine($"    mov eax, {arg1}");

            // 2. تنفيذ العملية
            string op = instruction.Op switch
            {
                OpCode.ADD => "add",
                OpCode.SUB => "sub",
                OpCode.MUL => "imul",
                _ => throw new InvalidOperationException($"Unsupported binary op: {instruction.Op}")
            };

            _assemblyCode.AppendLine($"    {op} eax, {arg2}");

            // 3. تخزين النتيجة
            _assemblyCode.AppendLine($"    mov {result}, eax");
        }

        private void TranslateRead(IntermediateInstruction instruction)
        {
            var dest = GetVariableLocation(instruction.Result);
            _assemblyCode.AppendLine($"; TODO: Implement Read (sys_read) into {dest}");
            // يتطلب تنفيذ دالة قراءة معقدة في Assembly
        }

        private void TranslatePrint(IntermediateInstruction instruction)
        {
            var source = instruction.Arg1;
            if (source.Value is string stringValue)
            {
                // طباعة سلسلة نصية
                _assemblyCode.AppendLine($"; TODO: Implement String Print for {stringValue}");
                // يتطلب تعريف السلسلة في قسم .data واستخدام sys_write
            }
            else
            {
                // طباعة متغير أو رقم (يتطلب تحويل رقمي إلى سلسلة ASCII)
                var sourceLoc = GetVariableLocation(source);
                _assemblyCode.AppendLine($"; TODO: Implement Integer Print for {sourceLoc}");
                // يتطلب دالة itoa (Integer to ASCII) واستخدام sys_write
            }
        }
    }
}
