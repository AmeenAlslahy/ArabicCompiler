using System.Collections.Generic;

namespace ArabicCompilerApp
{
    // Base class for all Abstract Syntax Tree (AST) nodes
    public abstract class AstNode
    {
        // Visitor pattern implementation
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }

    // Interface for the Visitor pattern
    public interface IAstVisitor<T>
    {
        T Visit(ProgramNode node);
        T Visit(VariableDeclarationNode node);
        T Visit(AssignmentStatementNode node);
        T Visit(ReadStatementNode node);
        T Visit(PrintStatementNode node);
        T Visit(BinaryExpressionNode node);
        T Visit(LiteralNode node);
        T Visit(IdentifierNode node);
        // Add more Visit methods for other statement/expression types as needed
    }

    // Program structure: برنامج ... ; { ... } .
    public class ProgramNode : AstNode
    {
        public string Name { get; set; }
        public List<AstNode> Statements { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    // Variable declaration: متغير x : صحيح ;
    public class VariableDeclarationNode : AstNode
    {
        public string Identifier { get; set; }
        public string Type { get; set; } // e.g., "صحيح"

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    // Assignment statement: y = 5 * ( x + 2 ) ;
    public class AssignmentStatementNode : AstNode
    {
        public string Identifier { get; set; }
        public AstNode Expression { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    // Read statement: اقرأ ( x ) ;
    public class ReadStatementNode : AstNode
    {
        public string Identifier { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    // Print statement: اطبع ( y , "النتيجة هي" ) ;
    public class PrintStatementNode : AstNode
    {
        public List<AstNode> Arguments { get; set; } // Can be expressions or literals

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    // Binary expression: 5 * ( x + 2 )
    public class BinaryExpressionNode : AstNode
    {
        public AstNode Left { get; set; }
        public string Operator { get; set; } // e.g., "*", "+"
        public AstNode Right { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    // Literal value: 5 or "النتيجة هي"
    public class LiteralNode : AstNode
    {
        public object Value { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    // Identifier (variable name): x, y
    public class IdentifierNode : AstNode
    {
        public string Name { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}
