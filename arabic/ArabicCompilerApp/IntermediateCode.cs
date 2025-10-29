using System.Collections.Generic;
using System.Text;

namespace ArabicCompiler
{
    // أنواع العمليات في الكود الوسيط
    public enum OpCode
    {
        // عمليات رياضية ومنطقية
        ADD, SUB, MUL, DIV, MOD, POW, AND, OR, NOT,
        
        // عمليات المقارنة
        EQ, NEQ, LT, GT, LTE, GTE,
        
        // عمليات النقل والإسناد
        ASSIGN,
        
        // عمليات التحكم في التدفق
        GOTO, IF_GOTO,
        
        // عمليات الإدخال والإخراج
        READ, PRINT,
        
        // عمليات خاصة بالدوال والإجراءات
        CALL, RETURN, PARAM,
        
        // عمليات تعريف التسميات
        LABEL,
        
        // نهاية الكود
        HALT
    }

    // فئة لتمثيل معامل (Operand) في الكود الوسيط
    // يمكن أن يكون متغير، ثابت، أو متغير مؤقت
    public class Operand
    {
        public string Name { get; }
        public object Value { get; }
        public bool IsTemporary { get; }

        // للمتغيرات والتسميات
        public Operand(string name, bool isTemporary = false)
        {
            Name = name;
            IsTemporary = isTemporary;
            Value = null;
        }

        // للثوابت
        public Operand(object value)
        {
            Name = value.ToString() ?? "null"; // يجب أن يكون القيمة غير فارغة
            Value = value;
            IsTemporary = false;
        }

        public override string ToString()
        {
            if (IsTemporary)
            {
                return $"T_{Name}";
            }
            if (Value != null)
            {
                // للثوابت، نستخدم القيمة مباشرة
                return Value is string ? $"\"{Value}\"" : Value.ToString() ?? Name;
            }
            return Name;
        }
    }

    // فئة لتمثيل تعليمة واحدة من الكود الوسيط (Three-Address Code)
    public class IntermediateInstruction
    {
        public OpCode Op { get; }
        public Operand Result { get; } // المعامل الذي يحمل النتيجة (مثل T1)
        public Operand Arg1 { get; } // المعامل الأول
        public Operand Arg2 { get; } // المعامل الثاني

        public IntermediateInstruction(OpCode op, Operand result, Operand arg1 = null, Operand arg2 = null)
        {
            Op = op;
            Result = result;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Op}");

            if (Result != null)
            {
                sb.Append($" {Result}");
            }
            if (Arg1 != null)
            {
                sb.Append($" {Arg1}");
            }
            if (Arg2 != null)
            {
                sb.Append($" {Arg2}");
            }

            return sb.ToString();
        }
    }

    // فئة لتوليد الكود الوسيط
    public class IntermediateCodeGenerator
    {
        private readonly List<IntermediateInstruction> _instructions = new List<IntermediateInstruction>();
        private int _tempCounter = 0;
        private int _labelCounter = 0;

        public List<IntermediateInstruction> Instructions => _instructions;

        // توليد متغير مؤقت جديد
        public Operand NewTemp()
        {
            return new Operand((_tempCounter++).ToString(), true);
        }

        // توليد تسمية جديدة
        public string NewLabel()
        {
            return $"L{_labelCounter++}";
        }

        // إضافة تعليمة إلى قائمة التعليمات
        public void Emit(OpCode op, Operand result, Operand arg1 = null, Operand arg2 = null)
        {
            _instructions.Add(new IntermediateInstruction(op, result, arg1, arg2));
        }

        // إضافة تسمية إلى قائمة التعليمات
        public void EmitLabel(string label)
        {
            Emit(OpCode.LABEL, new Operand(label));
        }

        // طباعة الكود الوسيط
        public string PrintCode()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _instructions.Count; i++)
            {
                var instruction = _instructions[i];
                if (instruction.Op == OpCode.LABEL)
                {
                    sb.AppendLine($"{instruction.Result}:");
                }
                else
                {
                    sb.AppendLine($"\t{i}: {instruction}");
                }
            }
            return sb.ToString();
        }
    }
}
