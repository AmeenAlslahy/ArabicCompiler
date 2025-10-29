using System;
using System.Collections.Generic;

namespace ArabicCompiler
{
    // أنواع البيانات في اللغة
    public enum DataType
    {
        Unknown, Integer, Real, Boolean, Char, String, Record, List, Procedure
    }

    // فئة لتمثيل إدخال في جدول الرموز
    public class Symbol
    {
        public string Name { get; }
        public DataType Type { get; set; }
        public int ScopeLevel { get; }
        // يمكن إضافة المزيد من الخصائص مثل: IsConstant, IsByReference, ParametersList, etc.

        public Symbol(string name, DataType type, int scopeLevel)
        {
            Name = name;
            Type = type;
            ScopeLevel = scopeLevel;
        }
    }

    // فئة لتمثيل جدول الرموز (Symbol Table)
    public class SymbolTable
    {
        private readonly Dictionary<string, Symbol> _symbols = new Dictionary<string, Symbol>();
        private readonly SymbolTable _parent;
        public int ScopeLevel { get; }

        public SymbolTable(SymbolTable parent = null)
        {
            _parent = parent;
            ScopeLevel = parent == null ? 0 : parent.ScopeLevel + 1;
        }

        public void Define(Symbol symbol)
        {
            if (_symbols.ContainsKey(symbol.Name))
            {
                // يجب أن تكون هناك آلية للإبلاغ عن خطأ "إعادة تعريف"
                throw new Exception($"Symbol '{symbol.Name}' is already defined in this scope.");
            }
            _symbols[symbol.Name] = symbol;
        }

        public Symbol Lookup(string name)
        {
            if (_symbols.TryGetValue(name, out var symbol))
            {
                return symbol;
            }
            if (_parent != null)
            {
                return _parent.Lookup(name);
            }
            return null; // لم يتم العثور على الرمز
        }
    }

    // استثناء خاص بالتحليل الدلالي
    public class SemanticException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public SemanticException(string message, AstNode node)
            : base($"Semantic Error at Line {node.Token.Line}, Column {node.Token.Column}: {message}")
        {
            Line = node.Token.Line;
            Column = node.Token.Column;
        }
    }

    // المحلل الدلالي (Semantic Analyzer) - باستخدام نمط Visitor
    public class SemanticAnalyzer
    {
        private SymbolTable _currentScope;

        public SemanticAnalyzer()
        {
            // إنشاء النطاق العام (Global Scope)
            _currentScope = new SymbolTable();
            // إضافة الأنواع الأساسية كنطاقات معرفة مسبقًا
            _currentScope.Define(new Symbol("صحيح", DataType.Integer, 0));
            _currentScope.Define(new Symbol("حقيقي", DataType.Real, 0));
            _currentScope.Define(new Symbol("منطقي", DataType.Boolean, 0));
            _currentScope.Define(new Symbol("حرفي", DataType.Char, 0));
            _currentScope.Define(new Symbol("خيط رمزي", DataType.String, 0));
        }

        public void Analyze(AstNode root)
        {
            // في مشروع متكامل، يجب أن يكون هناك فئة ProgramNode
            // حاليًا، نفترض أن الجذر هو StatementListNode
            if (root is StatementListNode statementList)
            {
                VisitStatementList(statementList);
            }
            // يجب إضافة زيارة لبقية العقد هنا
        }

        private void VisitStatementList(StatementListNode node)
        {
            foreach (var statement in node.Statements)
            {
                Visit(statement);
            }
        }

        private void Visit(AstNode node)
        {
            switch (node)
            {
                case AssignmentNode assign:
                    VisitAssignment(assign);
                    break;
                case VariableAccessNode varAccess:
                    VisitVariableAccess(varAccess);
                    break;
                case LiteralNode literal:
                    // لا شيء للتحقق منه في الثوابت
                    break;
                case BinaryOperationNode binary:
                    VisitBinaryOp(binary);
                    break;
                case UnaryOperationNode unary:
                    VisitUnaryOp(unary);
                    break;
                case PrintNode print:
                    VisitPrint(print);
                    break;
                case ReadNode read:
                    VisitRead(read);
                    break;
                default:
                    // يمكن إضافة المزيد من العقد هنا
                    break;
            }
        }

        private void VisitAssignment(AssignmentNode node)
        {
            // 1. التحقق من أن الطرف الأيسر هو متغير معرف
            if (!(node.Variable is VariableAccessNode varAccess))
            {
                throw new SemanticException("Left side of assignment must be a variable.", node);
            }

            var symbol = _currentScope.Lookup(varAccess.Name);
            if (symbol == null)
            {
                throw new SemanticException($"Variable '{varAccess.Name}' not declared.", node.Variable);
            }

            // 2. التحقق من توافق الأنواع (Type Checking)
            // في هذا المستوى، سنفترض أن الأنواع متوافقة للتبسيط، ولكن يجب تنفيذ منطق التحقق هنا
            // مثال: var leftType = symbol.Type; var rightType = GetExpressionType(node.Expression);
            // if (leftType != rightType && !IsImplicitlyConvertible(rightType, leftType)) { throw... }

            // زيارة التعبير للتأكد من صحته
            Visit(node.Expression);
        }

        private void VisitVariableAccess(VariableAccessNode node)
        {
            var symbol = _currentScope.Lookup(node.Name);
            if (symbol == null)
            {
                throw new SemanticException($"Variable '{node.Name}' not declared.", node);
            }
            // يمكن تخزين نوع الرمز في العقدة هنا (node.Type = symbol.Type)
        }

        private void VisitBinaryOp(BinaryOperationNode node)
        {
            Visit(node.Left);
            Visit(node.Right);

            // 1. التحقق من أنواع المعاملات (Type Checking)
            // 2. التحقق من أن العامل (Operator) صالح للأنواع
            // مثال: لا يمكن استخدام '+' مع نوع 'منطقي'
        }

        private void VisitUnaryOp(UnaryOperationNode node)
        {
            Visit(node.Operand);
            // التحقق من أن العامل (Operator) صالح للنوع
        }

        private void VisitPrint(PrintNode node)
        {
            foreach (var item in node.PrintItems)
            {
                Visit(item);
                // يمكن إضافة تحقق هنا للتأكد من أن العنصر قابل للطباعة
            }
        }

        private void VisitRead(ReadNode node)
        {
            if (!(node.Variable is VariableAccessNode varAccess))
            {
                throw new SemanticException("Read statement must target a variable.", node);
            }

            var symbol = _currentScope.Lookup(varAccess.Name);
            if (symbol == null)
            {
                throw new SemanticException($"Variable '{varAccess.Name}' not declared for reading.", node.Variable);
            }
        }

        // ----------------------------------------------------------------------
        // وظائف مساعدة (Helper Functions) - للتجربة فقط
        // ----------------------------------------------------------------------

        // وظيفة تجريبية لإضافة متغيرات إلى النطاق الحالي
        public void AddVariableToScope(string name, DataType type)
        {
            try
            {
                _currentScope.Define(new Symbol(name, type, _currentScope.ScopeLevel));
            }
            catch (Exception ex)
            {
                // يجب التعامل مع الاستثناءات بشكل أفضل
                Console.WriteLine(ex.Message);
            }
        }
    }
}
