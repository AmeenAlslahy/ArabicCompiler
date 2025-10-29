using System.Collections.Generic;
using System.Linq;

namespace ArabicCompiler
{
    // المحلل اللغوي (Lexical Analyzer)
    public class Lexer
    {
        private readonly string _sourceCode;
        private int _position;
        private int _line;
        private int _column;
        private char CurrentChar => _position < _sourceCode.Length ? _sourceCode[_position] : '\0';

        // جدول الكلمات المفتاحية (Keywords Table)
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            {"برنامج", TokenType.PROGRAM_KW},
            {"ثابت", TokenType.CONST_KW},
            {"نوع", TokenType.TYPE_KW},
            {"متغير", TokenType.VAR_KW},
            {"إجراء", TokenType.PROCEDURE_KW},
            {"قائمة", TokenType.LIST_KW},
            {"سجل", TokenType.RECORD_KW},
            {"من", TokenType.FROM_KW},
            {"اقرأ", TokenType.READ_KW},
            {"اطبع", TokenType.PRINT_KW},
            {"إذا", TokenType.IF_KW},
            {"فإن", TokenType.THEN_KW},
            {"وإلا", TokenType.ELSE_KW},
            {"كرر", TokenType.REPEAT_KW},
            {"إلى", TokenType.TO_KW},
            {"أضف", TokenType.ADD_KW},
            {"ماطال", TokenType.WHILE_KW},
            {"استمر", TokenType.CONTINUE_KW},
            {"عد", TokenType.REPEAT_KW}, // إعادة استخدام REPEAT_KW لـ "عد"
            {"حتى", TokenType.UNTIL_KW},
            {"بالقيمة", TokenType.BY_VALUE_KW},
            {"بالمرجع", TokenType.BY_REF_KW},
            {"صحيح", TokenType.INTEGER_KW},
            {"حقيقي", TokenType.REAL_KW},
            {"منطقي", TokenType.BOOLEAN_KW},
            {"حرفي", TokenType.CHAR_KW},
            {"خيط رمزي", TokenType.STRING_KW},
            {"صح", TokenType.TRUE_KW},
            {"خطأ", TokenType.FALSE_KW}
        };

        public Lexer(string sourceCode)
        {
            _sourceCode = sourceCode;
            _position = 0;
            _line = 1;
            _column = 1;
        }

        // تقدم المؤشر
        private void Advance(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (_position < _sourceCode.Length)
                {
                    if (_sourceCode[_position] == '\n')
                    {
                        _line++;
                        _column = 1;
                    }
                    else
                    {
                        _column++;
                    }
                    _position++;
                }
            }
        }

        // تخطي المسافات البيضاء
        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(CurrentChar))
            {
                Advance();
            }
        }

        // قراءة المعرفات والكلمات المفتاحية
        private Token ReadIdentifierOrKeyword()
        {
            var startPos = _position;
            var startCol = _column;
            var startLine = _line;

            // المعرف يبدأ بحرف ويتبعه حروف أو أرقام
            while (char.IsLetter(CurrentChar) || char.IsDigit(CurrentChar))
            {
                Advance();
            }

            var lexeme = _sourceCode.Substring(startPos, _position - startPos);

            // التحقق مما إذا كان المعرف كلمة مفتاحية
            if (Keywords.TryGetValue(lexeme, out var tokenType))
            {
                return new Token(tokenType, lexeme, null, startLine, startCol);
            }

            return new Token(TokenType.IDENTIFIER, lexeme, lexeme, startLine, startCol);
        }

        // قراءة الأرقام (صحيح وحقيقي)
        private Token ReadNumber()
        {
            var startPos = _position;
            var startCol = _column;
            var startLine = _line;
            var isReal = false;

            while (char.IsDigit(CurrentChar))
            {
                Advance();
            }

            // التحقق من وجود جزء عشري (النقطة .)
            if (CurrentChar == '.')
            {
                isReal = true;
                Advance(); // تخطي النقطة
                while (char.IsDigit(CurrentChar))
                {
                    Advance();
                }
            }

            var lexeme = _sourceCode.Substring(startPos, _position - startPos);

            if (isReal)
            {
                if (double.TryParse(lexeme, out var value))
                {
                    return new Token(TokenType.REAL_LITERAL, lexeme, value, startLine, startCol);
                }
            }
            else
            {
                if (int.TryParse(lexeme, out var value))
                {
                    return new Token(TokenType.INTEGER_LITERAL, lexeme, value, startLine, startCol);
                }
            }

            // في حالة فشل التحويل (رقم غير صالح)
            throw new LexerException($"Invalid number format: {lexeme}", startLine, startCol);
        }

        // قراءة السلاسل النصية (String Literals)
        private Token ReadStringLiteral()
        {
            var startCol = _column;
            var startLine = _line;
            Advance(); // تخطي علامة التنصيص المزدوجة (")

            var startPos = _position;
            while (CurrentChar != '"' && CurrentChar != '\0' && CurrentChar != '\n')
            {
                Advance();
            }

            if (CurrentChar == '\0' || CurrentChar == '\n')
            {
                throw new LexerException("Unterminated string literal.", startLine, startCol);
            }

            var value = _sourceCode.Substring(startPos, _position - startPos);
            Advance(); // تخطي علامة التنصيص المزدوجة الختامية

            return new Token(TokenType.STRING_LITERAL, $"\"{value}\"", value, startLine, startCol);
        }

        // قراءة الرمز المفرد (Char Literal)
        private Token ReadCharLiteral()
        {
            var startCol = _column;
            var startLine = _line;
            Advance(); // تخطي علامة التنصيص المفردة (`)

            if (CurrentChar == '\0' || CurrentChar == '\n')
            {
                throw new LexerException("Unterminated character literal.", startLine, startCol);
            }

            var charValue = CurrentChar;
            Advance(); // تخطي الحرف

            if (CurrentChar != '`')
            {
                throw new LexerException("Character literal must contain exactly one character and be terminated by `.", startLine, startCol);
            }
            Advance(); // تخطي علامة التنصيص المفردة الختامية

            return new Token(TokenType.CHAR_LITERAL, $"`{charValue}`", charValue, startLine, startCol);
        }

        // قراءة الرمز التالي
        public Token NextToken()
        {
            SkipWhitespace();

            if (_position >= _sourceCode.Length)
            {
                return new Token(TokenType.EOF, "EOF", null, _line, _column);
            }

            var startCol = _column;
            var startLine = _line;

            // المعرفات والكلمات المفتاحية
            if (char.IsLetter(CurrentChar))
            {
                return ReadIdentifierOrKeyword();
            }

            // الأرقام
            if (char.IsDigit(CurrentChar))
            {
                return ReadNumber();
            }

            // السلاسل النصية
            if (CurrentChar == '"')
            {
                return ReadStringLiteral();
            }

            // الرموز المفردة (Char)
            if (CurrentChar == '`')
            {
                return ReadCharLiteral();
            }

            // عوامل التشغيل والفواصل
            switch (CurrentChar)
            {
                case '.': Advance(); return new Token(TokenType.DOT, ".", null, startLine, startCol);
                case ':': Advance(); return new Token(TokenType.COLON, ":", null, startLine, startCol);
                case ';': Advance(); return new Token(TokenType.SEMICOLON, ";", null, startLine, startCol);
                case ',': Advance(); return new Token(TokenType.COMMA, ",", null, startLine, startCol);
                case '(': Advance(); return new Token(TokenType.LPAREN, "(", null, startLine, startCol);
                case ')': Advance(); return new Token(TokenType.RPAREN, ")", null, startLine, startCol);
                case '{': Advance(); return new Token(TokenType.LBRACE, "{", null, startLine, startCol);
                case '}': Advance(); return new Token(TokenType.RBRACE, "}", null, startLine, startCol);
                case '[': Advance(); return new Token(TokenType.LBRACKET, "[", null, startLine, startCol);
                case ']': Advance(); return new Token(TokenType.RBRACKET, "]", null, startLine, startCol);

                // عوامل التشغيل المركبة (مثل ==, !=, =<, =>, &&, ||)
                case '=':
                    Advance();
                    if (CurrentChar == '=') { Advance(); return new Token(TokenType.EQ, "==", null, startLine, startCol); }
                    if (CurrentChar == '>') { Advance(); return new Token(TokenType.GTE, "=>", null, startLine, startCol); }
                    if (CurrentChar == '<') { Advance(); return new Token(TokenType.LTE, "=<", null, startLine, startCol); }
                    return new Token(TokenType.ASSIGN, "=", null, startLine, startCol); // الإسناد =

                case '!':
                    Advance();
                    if (CurrentChar == '=') { Advance(); return new Token(TokenType.NEQ, "!=", null, startLine, startCol); }
                    return new Token(TokenType.NOT, "!", null, startLine, startCol); // النفي !

                case '<': Advance(); return new Token(TokenType.LT, "<", null, startLine, startCol);
                case '>': Advance(); return new Token(TokenType.GT, ">", null, startLine, startCol);
                case '+': Advance(); return new Token(TokenType.PLUS, "+", null, startLine, startCol);
                case '-': Advance(); return new Token(TokenType.MINUS, "-", null, startLine, startCol);
                case '*': Advance(); return new Token(TokenType.MULTIPLY, "*", null, startLine, startCol);
                case '/': Advance(); return new Token(TokenType.DIVIDE, "/", null, startLine, startCol);
                case '\\': Advance(); return new Token(TokenType.INT_DIVIDE, "\\", null, startLine, startCol);
                case '%': Advance(); return new Token(TokenType.MODULO, "%", null, startLine, startCol);
                case '^': Advance(); return new Token(TokenType.POWER, "^", null, startLine, startCol);

                case '&':
                    Advance();
                    if (CurrentChar == '&') { Advance(); return new Token(TokenType.AND, "&&", null, startLine, startCol); }
                    throw new LexerException($"Invalid token: &", startLine, startCol);

                case '|':
                    Advance();
                    if (CurrentChar == '|') { Advance(); return new Token(TokenType.OR, "||", null, startLine, startCol); }
                    throw new LexerException($"Invalid token: |", startLine, startCol);

                default:
                    var unknownChar = CurrentChar;
                    Advance();
                    throw new LexerException($"Unknown character: '{unknownChar}'", startLine, startCol);
            }
        }

        // للحصول على جميع الرموز (Tokens)
        public List<Token> GetAllTokens()
        {
            var tokens = new List<Token>();
            Token token;
            do
            {
                token = NextToken();
                tokens.Add(token);
            } while (token.Type != TokenType.EOF);

            return tokens;
        }
    }
}
