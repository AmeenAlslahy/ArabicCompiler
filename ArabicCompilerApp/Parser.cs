using System;
using System.Collections.Generic;
using System.Linq;

namespace ArabicCompiler
{
    // فئة أساسية لتمثيل عقدة في شجرة بناء الجملة المجردة (AST)
    public abstract class AstNode
    {
        public Token Token { get; }
        protected AstNode(Token token)
        {
            Token = token;
        }
    }

    // استثناء خاص بالمحلل النحوي
    public class ParserException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public ParserException(string message, Token token)
            : base($"Syntax Error at Line {token.Line}, Column {token.Column}: {message}")
        {
            Line = token.Line;
            Column = token.Column;
        }
    }

    // المحلل النحوي (Syntax Analyzer)
    public class Parser
    {
        private readonly Lexer _lexer;
        private Token _currentToken;

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.NextToken(); // قراءة أول رمز
        }

        // تقدم إلى الرمز التالي
        private void Consume()
        {
            _currentToken = _lexer.NextToken();
        }

        // التحقق من نوع الرمز الحالي والتقدم
        private void Expect(TokenType type, string errorMessage)
        {
            if (_currentToken.Type == type)
            {
                Consume();
            }
            else
            {
                throw new ParserException($"{errorMessage}. Expected {type}, got {_currentToken.Type}", _currentToken);
            }
        }

        // نقطة الدخول: تحليل البرنامج
        public AstNode ParseProgram()
        {
            // <برنامج> ::= برنامج <اسم برنامج> ; <كتلة برمجية> .
            Expect(TokenType.PROGRAM_KW, "Program must start with 'برنامج'");
            var programName = _currentToken;
            Expect(TokenType.IDENTIFIER, "Expected program name (identifier)");
            Expect(TokenType.SEMICOLON, "Expected ';' after program name");

            var block = ParseBlock();

            Expect(TokenType.DOT, "Program must end with '.'");
            Expect(TokenType.EOF, "Expected end of file");

            // هنا يجب أن تعود بعقدة البرنامج (ProgramNode)
            // لكن سنكتفي حاليًا بإرجاع الكتلة البرمجية للتبسيط
            return block;
        }

        // تحليل الكتلة البرمجية
        // <كتلة برمجية> ::= [<جزء التعريفات>] <قائمة التعليمات>
        private AstNode ParseBlock()
        {
            // جزء التعريفات (سنقوم بتنفيذه لاحقًا)
            ParseDeclarations();

            // قائمة التعليمات
            return ParseStatementList();
        }

        // تحليل جزء التعريفات (Placeholder)
        private void ParseDeclarations()
        {
            // يجب تنفيذ تحليل الثوابت، الأنواع، المتغيرات، والإجراءات هنا
            // حاليًا نكتفي بتخطيها إذا لم تكن موجودة
        }

        // تحليل قائمة التعليمات
        // <قائمة تعليمات> ::= { (<تعليمة> ;)* <تعليمة> }
        private AstNode ParseStatementList()
        {
            Expect(TokenType.LBRACE, "Expected '{' to start statement list");

            var statements = new List<AstNode>();
            while (_currentToken.Type != TokenType.RBRACE && _currentToken.Type != TokenType.EOF)
            {
                var statement = ParseStatement();
                statements.Add(statement);

                // التعليمات تفصل بـ ;
                if (_currentToken.Type == TokenType.SEMICOLON)
                {
                    Consume();
                }
                else if (_currentToken.Type != TokenType.RBRACE)
                {
                    throw new ParserException("Expected ';' to separate statements", _currentToken);
                }
            }

            Expect(TokenType.RBRACE, "Expected '}' to end statement list");

            // هنا يجب أن تعود بعقدة قائمة التعليمات (StatementListNode)
            // سنستخدم عقدة بسيطة حاليًا
            return new StatementListNode(statements);
        }

        // تحليل تعليمة واحدة (سنبدأ بتعليمة الإسناد والإخراج)
        private AstNode ParseStatement()
        {
            switch (_currentToken.Type)
            {
                case TokenType.IDENTIFIER:
                    return ParseAssignmentStatement();
                case TokenType.PRINT_KW:
                    return ParsePrintStatement();
                case TokenType.READ_KW:
                    return ParseReadStatement();
                // يمكن إضافة المزيد من التعليمات هنا (IF, REPEAT, etc.)
                default:
                    throw new ParserException($"Unexpected token while parsing statement: {_currentToken.Lexeme}", _currentToken);
            }
        }

        // تحليل تعليمة الإسناد
        // <تعليمة إسناد> ::= <متغير وصول> = <تعبير>
        private AstNode ParseAssignmentStatement()
        {
            var variable = ParseVariableAccess();
            Expect(TokenType.ASSIGN, "Expected '=' for assignment");
            var expression = ParseExpression();
            return new AssignmentNode(variable, expression);
        }

        // تحليل تعليمة الإخراج (اطبع)
        // <تعليمة إخراج> ::= اطبع ( <قائمة طباعة> )
        private AstNode ParsePrintStatement()
        {
            var printToken = _currentToken;
            Expect(TokenType.PRINT_KW, "Expected 'اطبع'");
            Expect(TokenType.LPAREN, "Expected '(' after 'اطبع'");

            var printItems = new List<AstNode>();
            // <قائمة طباعة> ::= (<عنصر طباعة> ,)* <عنصر طباعة>
            do
            {
                var item = ParsePrintItem();
                printItems.Add(item);
                if (_currentToken.Type == TokenType.COMMA)
                {
                    Consume();
                }
                else
                {
                    break;
                }
            } while (_currentToken.Type != TokenType.RPAREN);

            Expect(TokenType.RPAREN, "Expected ')' after print list");
            return new PrintNode(printToken, printItems);
        }

        // تحليل عنصر الطباعة
        // <عنصر طباعة> ::= <متغير وصول> | <حرفي>
        private AstNode ParsePrintItem()
        {
            if (_currentToken.Type == TokenType.STRING_LITERAL || _currentToken.Type == TokenType.CHAR_LITERAL)
            {
                var literal = new LiteralNode(_currentToken);
                Consume();
                return literal;
            }
            return ParseVariableAccess();
        }

        // تحليل تعليمة الإدخال (اقرأ)
        // <تعليمة إدخال> ::= اقرأ ( <متغير وصول> )
        private AstNode ParseReadStatement()
        {
            var readToken = _currentToken;
            Expect(TokenType.READ_KW, "Expected 'اقرأ'");
            Expect(TokenType.LPAREN, "Expected '(' after 'اقرأ'");
            var variable = ParseVariableAccess();
            Expect(TokenType.RPAREN, "Expected ')' after variable name");
            return new ReadNode(readToken, variable);
        }

        // تحليل متغير الوصول (Variable Access)
        // <متغير وصول> ::= <اسم متغير> [ <مختار> ]*
        private AstNode ParseVariableAccess()
        {
            var identifierToken = _currentToken;
            Expect(TokenType.IDENTIFIER, "Expected variable identifier");
            
            // حاليًا نكتفي بالمتغير البسيط دون مختار (Field/Index Selector)
            return new VariableAccessNode(identifierToken);
        }

        // تحليل التعبير (Expression)
        // <تعبير> ::= <تعبير بسيط> [ <عامل ربط> <تعبير بسيط> ]
        private AstNode ParseExpression()
        {
            var left = ParseSimpleExpression();

            // عوامل الربط (المقارنات والمنطق)
            if (new[] { TokenType.EQ, TokenType.NEQ, TokenType.LT, TokenType.GT, TokenType.LTE, TokenType.GTE, TokenType.AND, TokenType.OR }.Contains(_currentToken.Type))
            {
                var op = _currentToken;
                Consume();
                var right = ParseSimpleExpression();
                return new BinaryOperationNode(op, left, right);
            }

            return left;
        }

        // تحليل التعبير البسيط
        // <تعبير بسيط> ::= [ <عامل إشارة> ] <حد> ( <عامل جمع> <حد> )*
        private AstNode ParseSimpleExpression()
        {
            Token unaryOp = null;
            // عامل الإشارة (Unary Operator)
            if (_currentToken.Type == TokenType.PLUS || _currentToken.Type == TokenType.MINUS)
            {
                unaryOp = _currentToken;
                Consume();
            }

            var left = ParseTerm();
            if (unaryOp != null)
            {
                left = new UnaryOperationNode(unaryOp, left);
            }

            // عوامل الجمع والطرح
            while (_currentToken.Type == TokenType.PLUS || _currentToken.Type == TokenType.MINUS)
            {
                var op = _currentToken;
                Consume();
                var right = ParseTerm();
                left = new BinaryOperationNode(op, left, right);
            }

            return left;
        }

        // تحليل الحد (Term)
        // <حد> ::= <عامل> ( <عامل ضرب> <عامل> )*
        private AstNode ParseTerm()
        {
            var left = ParseFactor();

            // عوامل الضرب والقسمة
            while (new[] { TokenType.MULTIPLY, TokenType.DIVIDE, TokenType.INT_DIVIDE, TokenType.MODULO, TokenType.POWER }.Contains(_currentToken.Type))
            {
                var op = _currentToken;
                Consume();
                var right = ParseFactor();
                left = new BinaryOperationNode(op, left, right);
            }

            return left;
        }

        // تحليل العامل (Factor)
        // <عامل> ::= <قيمة ثابتة> | <متغير وصول> | ( <تعبير> ) | ! <عامل>
        private AstNode ParseFactor()
        {
            switch (_currentToken.Type)
            {
                case TokenType.INTEGER_LITERAL:
                case TokenType.REAL_LITERAL:
                case TokenType.CHAR_LITERAL:
                case TokenType.STRING_LITERAL:
                case TokenType.TRUE_KW:
                case TokenType.FALSE_KW:
                    var literal = new LiteralNode(_currentToken);
                    Consume();
                    return literal;

                case TokenType.IDENTIFIER:
                    return ParseVariableAccess(); // يمكن أن يكون متغير وصول أو استدعاء إجراء بدون معلمات

                case TokenType.LPAREN:
                    Consume(); // تخطي (
                    var expr = ParseExpression();
                    Expect(TokenType.RPAREN, "Expected ')' to close expression");
                    return expr;

                case TokenType.NOT:
                    var notOp = _currentToken;
                    Consume();
                    var factor = ParseFactor();
                    return new UnaryOperationNode(notOp, factor);

                default:
                    throw new ParserException($"Unexpected token while parsing factor: {_currentToken.Lexeme}", _currentToken);
            }
        }
    }

    // ----------------------------------------------------------------------
    // تعريف عقد شجرة بناء الجملة المجردة (AST Nodes)
    // ----------------------------------------------------------------------

    public class StatementListNode : AstNode
    {
        public List<AstNode> Statements { get; }
        public StatementListNode(List<AstNode> statements) : base(null)
        {
            Statements = statements;
        }
    }

    public class AssignmentNode : AstNode
    {
        public AstNode Variable { get; }
        public AstNode Expression { get; }
        public AssignmentNode(AstNode variable, AstNode expression) : base(null)
        {
            Variable = variable;
            Expression = expression;
        }
    }

    public class PrintNode : AstNode
    {
        public List<AstNode> PrintItems { get; }
        public PrintNode(Token token, List<AstNode> printItems) : base(token)
        {
            PrintItems = printItems;
        }
    }

    public class ReadNode : AstNode
    {
        public AstNode Variable { get; }
        public ReadNode(Token token, AstNode variable) : base(token)
        {
            Variable = variable;
        }
    }

    public class VariableAccessNode : AstNode
    {
        public string Name => Token.Lexeme;
        public VariableAccessNode(Token token) : base(token) { }
    }

    public class LiteralNode : AstNode
    {
        public object Value => Token.Value;
        public LiteralNode(Token token) : base(token) { }
    }

    public class BinaryOperationNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }
        public BinaryOperationNode(Token op, AstNode left, AstNode right) : base(op)
        {
            Left = left;
            Right = right;
        }
    }

    public class UnaryOperationNode : AstNode
    {
        public AstNode Operand { get; }
        public UnaryOperationNode(Token op, AstNode operand) : base(op)
        {
            Operand = operand;
        }
    }
}
