using System;
using System.Collections.Generic;

namespace ArabicCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Arabic Compiler IDE (Console Test Mode)");

            // مثال محدث حسب قواعد اللغة
            string source = @"
برنامج TestProgram ;
{
    متغير x : صحيح ;
    متغير y : حقيقي ;
    
    اقرأ ( x ) ;
    y = 5.5 + ( 10 =! 5 ) ;
    اطبع ( y , ""القيمة هي"" ) ;
} .
";
            
            try
            {
                // 1. التحليل اللغوي (Lexer)
                var lexer = new Lexer(source);
                Console.WriteLine("\n--- Lexical Analysis (Tokens) ---");
                var tokens = lexer.GetAllTokens();
                foreach (var token in tokens)
                {
                    Console.WriteLine(token);
                }

                // 2. التحليل النحوي (Parser)
                lexer = new Lexer(source); 
                var parser = new Parser(lexer);
                Console.WriteLine("\n--- Syntax Analysis (AST) ---");
                var ast = parser.ParseProgram();
                Console.WriteLine($"AST Root Node: {ast.GetType().Name}");

                // 3. التحليل الدلالي (Semantic Analyzer)
                var semanticAnalyzer = new SemanticAnalyzer();
                semanticAnalyzer.AddVariableToScope("x", DataType.Integer);
                semanticAnalyzer.AddVariableToScope("y", DataType.Real);

                Console.WriteLine("\n--- Semantic Analysis ---");
                semanticAnalyzer.Analyze(ast);
                Console.WriteLine("Semantic analysis completed successfully.");

                // 4. توليد الكود الوسيط
                var codeGenerator = new CodeGenerator(semanticAnalyzer);
                var intermediateCode = codeGenerator.Generate(ast);

                Console.WriteLine("\n--- Intermediate Code ---");
                Console.WriteLine(intermediateCode.PrintCode());

                // 5. توليد كود التجميع
                var assemblyGenerator = new AssemblyCodeGenerator(intermediateCode);
                var assemblyCode = assemblyGenerator.Generate();

                Console.WriteLine("\n--- Target Code (x86 Assembly) ---");
                Console.WriteLine(assemblyCode);
            }
            catch (LexerException ex)
            {
                Console.WriteLine($"\n[LEXER ERROR]: {ex.Message}");
            }
            catch (ParserException ex)
            {
                Console.WriteLine($"\n[PARSER ERROR]: {ex.Message}");
            }
            catch (SemanticException ex)
            {
                Console.WriteLine($"\n[SEMANTIC ERROR]: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[GENERAL ERROR]: {ex.Message}");
            }
        }
    }
}