using System.Collections.Generic;

namespace ArabicCompiler
{
    // أنواع الرموز (Token Types)
    public enum TokenType
    {
        // الكلمات المفتاحية (Keywords) - محدثة حسب قواعد اللغة
        PROGRAM_KW, CONST_KW, TYPE_KW, VAR_KW, PROCEDURE_KW,
        LIST_KW, RECORD_KW, FROM_KW, READ_KW, PRINT_KW,
        IF_KW, THEN_KW, ELSE_KW, REPEAT_KW, TO_KW, ADD_KW,
        WHILE_KW, CONTINUE_KW, UNTIL_KW, BY_VALUE_KW, BY_REF_KW,
        INTEGER_KW, REAL_KW, BOOLEAN_KW, CHAR_KW, STRING_KW,
        TRUE_KW, FALSE_KW,

        // المعرفات (Identifiers)
        IDENTIFIER,

        // الثوابت (Literals)
        INTEGER_LITERAL, REAL_LITERAL, CHAR_LITERAL, STRING_LITERAL,

        // عوامل التشغيل (Operators) - مصححة حسب الصفحة 9
        ASSIGN, EQ, NEQ, LT, GT, LTE, GTE, // =, ==, =!, <, >, =>, => 
        PLUS, MINUS, MULTIPLY, DIVIDE, INT_DIVIDE, MODULO, POWER, // +, -, *, /, \, %, ^
        AND, OR, NOT, // &&, ||, !

        // الفواصل والرموز (Separators and Punctuation) - مكتملة
        SEMICOLON, COMMA, COLON, DOT, // ;, ,, :, .
        LPAREN, RPAREN, LBRACE, RBRACE, LBRACKET, RBRACKET, // (, ), {, }, [, ]

        // نهاية الملف (End of File)
        EOF
    }

    // هيكل الرمز (Token Structure)
    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object? Value { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenType type, string lexeme, object? value, int line, int column)
        {
            Type = type;
            Lexeme = lexeme;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            var valueStr = Value != null ? $" = {Value}" : "";
            return $"[{Type}] '{Lexeme}'{valueStr} at ({Line}:{Column})";
        }

        // خصائص مساعدة
        public bool IsLiteral => Type == TokenType.INTEGER_LITERAL || 
                                Type == TokenType.REAL_LITERAL ||
                                Type == TokenType.CHAR_LITERAL || 
                                Type == TokenType.STRING_LITERAL;

        public bool IsKeyword => Type >= TokenType.PROGRAM_KW && Type <= TokenType.FALSE_KW;
    }
}