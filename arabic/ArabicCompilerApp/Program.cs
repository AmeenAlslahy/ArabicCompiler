using System;
using System.Collections.Generic;

namespace ArabicCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Arabic Compiler IDE (Console Test Mode)");

            // مثال بسيط للكود المصدري
            string source = @"
برنامج TestProgram ;
{
    اقرأ ( x ) ;
    y = 5 ;
    z = 10 + ( 5 * 2 ) ;
    اطبع ( z , ""القيمة هي"" ) ;
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
                // إعادة إنشاء Lexer لأن GetAllTokens استهلكت الـ source
                lexer = new Lexer(source); 
                var parser = new Parser(lexer);
                Console.WriteLine("\n--- Syntax Analysis (AST) ---");
                var ast = parser.ParseProgram();
                Console.WriteLine($"AST Root Node: {ast.GetType().Name}");

                // 3. التحليل الدلالي (Semantic Analyzer) - اختبار مبدئي
                var semanticAnalyzer = new SemanticAnalyzer();
                // إضافة متغيرات اختبارية للنطاق (للتغلب على عدم وجود محلل تعريفات كامل)
                semanticAnalyzer.AddVariableToScope("x", DataType.Integer);
                semanticAnalyzer.AddVariableToScope("y", DataType.Integer);
                semanticAnalyzer.AddVariableToScope("z", DataType.Integer);

                Console.WriteLine("\n--- Semantic Analysis ---");
                semanticAnalyzer.Analyze(ast);
                Console.WriteLine("Semantic analysis completed successfully (no errors found).");

                // 4. توليد الكود الوسيط (Intermediate Code Generation)
                var codeGenerator = new CodeGenerator(semanticAnalyzer);
                var intermediateCode = codeGenerator.Generate(ast);

                Console.WriteLine("\n--- Intermediate Code (Three-Address Code) ---");
                Console.WriteLine(intermediateCode.PrintCode());

                // 5. توليد كود التجميع (Assembly Code Generation)
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
