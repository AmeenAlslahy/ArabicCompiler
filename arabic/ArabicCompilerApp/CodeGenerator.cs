using System;
using System.Collections.Generic;

namespace ArabicCompiler
{
    // فئة لتوليد الكود الوسيط من شجرة AST
    public class CodeGenerator
    {
        private readonly IntermediateCodeGenerator _icg;
        private readonly SemanticAnalyzer _analyzer; // يمكن استخدام المحلل الدلالي للحصول على معلومات الرمز

        public CodeGenerator(SemanticAnalyzer analyzer)
        {
            _icg = new IntermediateCodeGenerator();
            _analyzer = analyzer;
        }

        public IntermediateCodeGenerator Generate(AstNode root)
        {
            // نفترض أن الجذر هو StatementListNode
            if (root is StatementListNode statementList)
            {
                VisitStatementList(statementList);
            }
            
            _icg.Emit(OpCode.HALT, null);
            return _icg;
        }

        private void VisitStatementList(StatementListNode node)
        {
            foreach (var statement in node.Statements)
            {
                Visit(statement);
            }
        }

        // الوظيفة الرئيسية للزيارة
        private Operand Visit(AstNode node)
        {
            switch (node)
            {
                case AssignmentNode assign:
                    VisitAssignment(assign);
                    return null;
                case PrintNode print:
                    VisitPrint(print);
                    return null;
                case ReadNode read:
                    VisitRead(read);
                    return null;
                case BinaryOperationNode binary:
                    return VisitBinaryOp(binary);
                case UnaryOperationNode unary:
                    return VisitUnaryOp(unary);
                case VariableAccessNode varAccess:
                    return VisitVariableAccess(varAccess);
                case LiteralNode literal:
                    return VisitLiteral(literal);
                default:
                    throw new NotImplementedException($"Code generation not implemented for node type: {node.GetType().Name}");
            }
        }

        private void VisitAssignment(AssignmentNode node)
        {
            // 1. حساب قيمة التعبير في الطرف الأيمن
            var rightOperand = Visit(node.Expression);

            // 2. الحصول على معامل المتغير في الطرف الأيسر
            var varAccess = (VariableAccessNode)node.Variable;
            var leftOperand = new Operand(varAccess.Name);

            // 3. توليد تعليمة الإسناد
            _icg.Emit(OpCode.ASSIGN, leftOperand, rightOperand);
        }

        private void VisitPrint(PrintNode node)
        {
            foreach (var item in node.PrintItems)
            {
                var operand = Visit(item);
                _icg.Emit(OpCode.PRINT, null, operand);
            }
        }

        private void VisitRead(ReadNode node)
        {
            var varAccess = (VariableAccessNode)node.Variable;
            var operand = new Operand(varAccess.Name);
            _icg.Emit(OpCode.READ, operand);
        }

        private Operand VisitBinaryOp(BinaryOperationNode node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            var result = _icg.NewTemp();

            OpCode opCode = GetOpCode(node.Token.Type);
            _icg.Emit(opCode, result, left, right);

            return result;
        }

        private Operand VisitUnaryOp(UnaryOperationNode node)
        {
            var operand = Visit(node.Operand);
            var result = _icg.NewTemp();

            OpCode opCode = GetOpCode(node.Token.Type);
            _icg.Emit(opCode, result, operand);

            return result;
        }

        private Operand VisitVariableAccess(VariableAccessNode node)
        {
            // ببساطة نرجع معامل يمثل المتغير
            return new Operand(node.Name);
        }

        private Operand VisitLiteral(LiteralNode node)
        {
            // ببساطة نرجع معامل يمثل الثابت
            return new Operand(node.Value);
        }

        private OpCode GetOpCode(TokenType type)
        {
            return type switch
            {
                // العمليات الرياضية
                TokenType.PLUS => OpCode.ADD,
                TokenType.MINUS => OpCode.SUB,
                TokenType.MULTIPLY => OpCode.MUL,
                TokenType.DIVIDE => OpCode.DIV,
                TokenType.MODULO => OpCode.MOD,
                TokenType.POWER => OpCode.POW,
                
                // العمليات المنطقية
                TokenType.AND => OpCode.AND,
                TokenType.OR => OpCode.OR,
                TokenType.NOT => OpCode.NOT,

                // عمليات المقارنة
                TokenType.EQ => OpCode.EQ,
                TokenType.NEQ => OpCode.NEQ,
                TokenType.LT => OpCode.LT,
                TokenType.GT => OpCode.GT,
                TokenType.LTE => OpCode.LTE,
                TokenType.GTE => OpCode.GTE,
                
                _ => throw new InvalidOperationException($"Unsupported token type for OpCode: {type}")
            };
        }
    }
}
