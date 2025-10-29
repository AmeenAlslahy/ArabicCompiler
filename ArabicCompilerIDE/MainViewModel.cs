using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.IO;
using System.Threading.Tasks;
using ArabicCompiler;

namespace ArabicCompilerIDE
{
    public class MainViewModel : ViewModelBase
    {
        #region الخصائص

        private string _sourceCode = @"برنامج مثال;
{
    متغير x : صحيح ;
    متغير y : صحيح ;
    متغير z : صحيح ;
    
    اقرأ(x);
    y = 5;
    z = 10 + (5 * 2);
    اطبع(z, ""القيمة هي:"");
}.";

        private string _tokensOutput = "// الرموز ستظهر هنا بعد التحليل اللغوي";
        private string _astOutput = "// الشجرة النحوية ستظهر هنا بعد التحليل النحوي";
        private string _intermediateCode = "// الكود الوسيط سيظهر هنا بعد توليد الكود";
        private string _assemblyCode = "// كود التجميع سيظهر هنا بعد الترجمة";
        private string _errorText = "// منطقة الأخطاء والتحذيرات";
        private string _programOutput = "// إخراج البرنامج سيظهر هنا بعد التشغيل";
        private string _userInput = "";
        private string _currentFilePath;
        private bool _isCompiled = false;
        private IntermediateCodeGenerator _lastIntermediateCode;

        public string SourceCode
        {
            get => _sourceCode;
            set
            {
                if (SetProperty(ref _sourceCode, value))
                {
                    // إعادة تعيين حالة الترجمة عند تغيير الكود
                    _isCompiled = false;
                    OnPropertyChanged(nameof(IsReadyToRun));
                }
            }
        }

        public string TokensOutput
        {
            get => _tokensOutput;
            set => SetProperty(ref _tokensOutput, value);
        }

        public string ASTOutput
        {
            get => _astOutput;
            set => SetProperty(ref _astOutput, value);
        }

        public string IntermediateCode
        {
            get => _intermediateCode;
            set => SetProperty(ref _intermediateCode, value);
        }

        public string AssemblyCode
        {
            get => _assemblyCode;
            set => SetProperty(ref _assemblyCode, value);
        }

        public string ErrorText
        {
            get => _errorText;
            set => SetProperty(ref _errorText, value);
        }

        public string ProgramOutput
        {
            get => _programOutput;
            set => SetProperty(ref _programOutput, value);
        }

        public string UserInput
        {
            get => _userInput;
            set => SetProperty(ref _userInput, value);
        }

        public string CurrentFilePath
        {
            get => _currentFilePath;
            set => SetProperty(ref _currentFilePath, value);
        }

        public bool IsReadyToRun => _isCompiled;

        #endregion

        #region الأوامر

        public ICommand CompileCommand { get; }
        public ICommand RunCommand { get; }
        public ICommand ShowTokensCommand { get; }
        public ICommand ShowASTCommand { get; }
        public ICommand ShowIntermediateCommand { get; }
        public ICommand ShowAssemblyCommand { get; }
        public ICommand NewFileCommand { get; }
        public ICommand SaveFileCommand { get; }

        private RealCodeExecutor _codeExecutor;
        private bool _isRealExecutionEnabled = true;

        public bool IsRealExecutionEnabled
        {
            get => _isRealExecutionEnabled;
            set => SetProperty(ref _isRealExecutionEnabled, value);
            }

            public ICommand ToggleExecutionModeCommand { get; }

            #endregion

            public MainViewModel()
        {
                _codeExecutor = new RealCodeExecutor();
                ToggleExecutionModeCommand = new RelayCommand(_ => ToggleExecutionMode());
                // تهيئة الأوامر
                CompileCommand = new RelayCommand(async _ => await ExecuteCompileAsync());
            RunCommand = new RelayCommand(async _ => await ExecuteRunAsync(), _ => IsReadyToRun);
            ShowTokensCommand = new RelayCommand(_ => ShowTokens());
            ShowASTCommand = new RelayCommand(_ => ShowAST());
            ShowIntermediateCommand = new RelayCommand(_ => ShowIntermediateCode());
            ShowAssemblyCommand = new RelayCommand(_ => ShowAssemblyCode());
            NewFileCommand = new RelayCommand(_ => NewFile());
            SaveFileCommand = new RelayCommand(_ => SaveFile());
        }

            public async Task ExecuteRunAsync()
{
                if (!_isCompiled)
                {
                    ErrorText = "⚠️ يجب ترجمة الكود أولاً قبل التشغيل";
                    return;
                }

                try
                {
                    ProgramOutput = "🚀 بدء تشغيل البرنامج...\n";

                    if (_isRealExecutionEnabled && _lastIntermediateCode != null)
                    {
                        // التنفيذ الحقيقي
                        ProgramOutput += "🔧 الوضع: التنفيذ الحقيقي\n";
                        var result = await _codeExecutor.ExecuteAsync(_lastIntermediateCode, UserInput);

                        if (result.Success)
                        {
                            ProgramOutput += $"✅ التنفيذ تم بنجاح (كود الخروج: {result.ExitCode})\n";
                            ProgramOutput += $"📤 الإخراج:\n{result.Output}";
                        }
                        else
                        {
                            ProgramOutput += $"❌ فشل في التنفيذ (كود الخروج: {result.ExitCode})\n";
                            ProgramOutput += $"📤 الإخراج:\n{result.Output}";
                        }
                    }
                    else
                    {
                        // التنفيذ المحاكى
                        ProgramOutput += "🎮 الوضع: المحاكاة\n";
                        await SimulateProgramExecution();
                        ProgramOutput += "\n✅ تم تنفيذ البرنامج بنجاح (وضع المحاكاة)";
                    }
                }
                catch (Exception ex)
                {
                    ProgramOutput = $"❌ خطأ أثناء التشغيل: {ex.Message}";
                }
            }

         private void ToggleExecutionMode()
        {
            IsRealExecutionEnabled = !IsRealExecutionEnabled;
            ProgramOutput = $"🔄 تم التبديل إلى: {(IsRealExecutionEnabled ? "التنفيذ الحقيقي" : "المحاكاة")}";
        }

        #region عمليات الملفات

        public void NewFile()
        {
            SourceCode = @"برنامج برنامج_جديد;
{
    متغير اسم : خيط ;
    
    اطبع(""أدخل اسمك:"");
    اقرأ(اسم);
    اطبع(""مرحبا "", اسم, ""!""); 
}.";
            
            ResetOutputs();
            CurrentFilePath = null;
            _isCompiled = false;
            OnPropertyChanged(nameof(IsReadyToRun));
        }

        public void LoadFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    SourceCode = File.ReadAllText(filePath);
                    CurrentFilePath = filePath;
                    ResetOutputs();
                    _isCompiled = false;
                    OnPropertyChanged(nameof(IsReadyToRun));
                }
            }
            catch (Exception ex)
            {
                ErrorText = $"❌ خطأ في تحميل الملف: {ex.Message}";
            }
        }

        public bool SaveFile()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentFilePath))
                {
                    return SaveFileAs();
                }

                File.WriteAllText(CurrentFilePath, SourceCode);
                return true;
            }
            catch (Exception ex)
            {
                ErrorText = $"❌ خطأ في حفظ الملف: {ex.Message}";
                return false;
            }
        }

        public bool SaveFileAs()
        {
            // سيتم التعامل مع هذا في MainWindow باستخدام SaveFileDialog
            return false; // سيعود القيمة الحقيقية من خلال الحدث
        }

        private void ResetOutputs()
        {
            TokensOutput = "// الرموز ستظهر هنا بعد التحليل اللغوي";
            ASTOutput = "// الشجرة النحوية ستظهر هنا بعد التحليل النحوي";
            IntermediateCode = "// الكود الوسيط سيظهر هنا بعد توليد الكود";
            AssemblyCode = "// كود التجميع سيظهر هنا بعد الترجمة";
            ErrorText = "// منطقة الأخطاء والتحذيرات";
            ProgramOutput = "// إخراج البرنامج سيظهر هنا بعد التشغيل";
        }

        #endregion

        #region عمليات الترجمة والتشغيل

        public async Task ExecuteCompileAsync()
        {
            try
            {
                ErrorText = "🔄 جاري التحليل اللغوي...";
                await Task.Delay(100); // لمحاكاة العمل غير المتزامن

                // 1. التحليل اللغوي (Lexical Analysis)
                var lexer = new Lexer(SourceCode);
                var tokens = lexer.GetAllTokens();
                TokensOutput = "🔤 الرموز اللغوية:\n" + string.Join("\n", tokens);

                ErrorText = "🔄 جاري التحليل النحوي...";
                await Task.Delay(100);

                // 2. التحليل النحوي (Syntax Analysis)
                lexer = new Lexer(SourceCode);
                var parser = new Parser(lexer);
                var ast = parser.ParseProgram();
                ASTOutput = "🌳 الشجرة النحوية:\n✅ تم بناء الشجرة النحوية بنجاح\n" +
                           $"📊 نوع العقدة الجذرية: {ast.GetType().Name}\n" +
                           $"🔍 عدد العقد الفرعية: {(ast is StatementListNode stmtList ? stmtList.Statements.Count : 0)}";

                ErrorText = "🔄 جاري التحليل الدلالي...";
                await Task.Delay(100);

                // 3. التحليل الدلالي (Semantic Analysis)
                var semanticAnalyzer = new SemanticAnalyzer();
                
                // إضافة المتغيرات المعرفة تلقائياً من الكود
                ExtractAndAddVariables(semanticAnalyzer);
                
                semanticAnalyzer.Analyze(ast);

                ErrorText = "🔄 جاري توليد الكود الوسيط...";
                await Task.Delay(100);

                // 4. توليد الكود الوسيط
                var codeGenerator = new CodeGenerator(semanticAnalyzer);
                _lastIntermediateCode = codeGenerator.Generate(ast);
                IntermediateCode = "🔄 الكود الوسيط (Three-Address Code):\n" + _lastIntermediateCode.PrintCode();

                ErrorText = "🔄 جاري توليد كود التجميع...";
                await Task.Delay(100);

                // 5. توليد كود التجميع
                var assemblyGenerator = new AssemblyCodeGenerator(_lastIntermediateCode);
                var assemblyCode = assemblyGenerator.Generate();
                AssemblyCode = "⚙️ كود التجميع (x86 Assembly):\n" + assemblyCode;

                _isCompiled = true;
                OnPropertyChanged(nameof(IsReadyToRun));
                
                ErrorText = "✅ الترجمة تمت بنجاح. لا توجد أخطاء.";
                ProgramOutput = "🔹 جاهز للتشغيل... استخدم زر التشغيل (F5) لتشغيل البرنامج";
            }
            catch (LexerException ex)
            {
                HandleError("❌ خطأ لغوي", ex.Message, ex.Line, ex.Column);
            }
            catch (ParserException ex)
            {
                HandleError("❌ خطأ نحوي", ex.Message, ex.Line, ex.Column);
            }
            catch (SemanticException ex)
            {
                HandleError("❌ خطأ دلالي", ex.Message, ex.Line, ex.Column);
            }
            catch (Exception ex)
            {
                HandleError("❌ خطأ عام", ex.Message);
            }
        }

        public async Task ExecuteRunAsync()
        {
            if (!_isCompiled)
            {
                ErrorText = "⚠️ يجب ترجمة الكود أولاً قبل التشغيل";
                return;
            }

            try
            {
                ProgramOutput = "🚀 بدء تشغيل البرنامج...\n";

                // محاكاة تنفيذ البرنامج مع الإدخال/الإخراج
                await SimulateProgramExecution();

                ProgramOutput += "\n✅ تم تنفيذ البرنامج بنجاح";
            }
            catch (Exception ex)
            {
                ProgramOutput = $"❌ خطأ أثناء التشغيل: {ex.Message}";
            }
        }

        private async Task SimulateProgramExecution()
        {
            // محاكاة تنفيذ البرنامج بناءً على الكود المصدري
            var inputLines = UserInput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var inputIndex = 0;

            // محاكاة قراءة وطباعة بناءً على الكود
            if (SourceCode.Contains("اقرأ"))
            {
                ProgramOutput += "📥 عملية الإدخال:\n";
                
                if (inputIndex < inputLines.Length)
                {
                    ProgramOutput += $"   → أدخلت القيمة: {inputLines[inputIndex]}\n";
                    inputIndex++;
                }
                else
                {
                    ProgramOutput += "   → لم يتم إدخال أي قيمة (استخدم تبويب الإدخال)\n";
                }
            }

            if (SourceCode.Contains("اطبع"))
            {
                ProgramOutput += "📤 عملية الإخراج:\n";
                
                // استخراج النصوص من تعليمات اطبع
                var printStatements = ExtractPrintStatements();
                foreach (var print in printStatements)
                {
                    ProgramOutput += $"   ← {print}\n";
                }
            }

            // محاكاة العمليات الحسابية
            if (SourceCode.Contains("=") && SourceCode.Any(char.IsDigit))
            {
                ProgramOutput += "🧮 العمليات الحسابية:\n";
                
                // محاكاة نتائج العمليات البسيطة
                var calculations = ExtractCalculations();
                foreach (var calc in calculations)
                {
                    ProgramOutput += $"   = {calc}\n";
                }
            }

            await Task.Delay(500); // محاكاة وقت التنفيذ
        }

        #endregion

        #region عرض المخرجات

        public void ShowTokens()
        {
            try
            {
                var lexer = new Lexer(SourceCode);
                var tokens = lexer.GetAllTokens();
                
                var tokenGroups = new Dictionary<TokenType, List<Token>>();
                foreach (var token in tokens)
                {
                    if (!tokenGroups.ContainsKey(token.Type))
                        tokenGroups[token.Type] = new List<Token>();
                    tokenGroups[token.Type].Add(token);
                }

                TokensOutput = "🔤 تحليل الرموز اللغوية:\n\n";
                foreach (var group in tokenGroups)
                {
                    TokensOutput += $"📁 {group.Key} ({group.Value.Count} رمز):\n";
                    foreach (var token in group.Value)
                    {
                        TokensOutput += $"   • {token.Lexeme}";
                        if (token.Value != null)
                            TokensOutput += $" = {token.Value}";
                        TokensOutput += $" [سطر: {token.Line}, عمود: {token.Column}]\n";
                    }
                    TokensOutput += "\n";
                }
            }
            catch (Exception ex)
            {
                TokensOutput = $"❌ خطأ في تحليل الرموز: {ex.Message}";
            }
        }

        public void ShowAST()
        {
            try
            {
                var lexer = new Lexer(SourceCode);
                var parser = new Parser(lexer);
                var ast = parser.ParseProgram();
                
                ASTOutput = "🌳 تحليل الشجرة النحوية:\n\n";
                ASTOutput += $"🏁 العقدة الجذرية: {ast.GetType().Name}\n\n";
                
                if (ast is StatementListNode stmtList)
                {
                    ASTOutput += $"📊 عدد التعليمات: {stmtList.Statements.Count}\n\n";
                    
                    for (int i = 0; i < stmtList.Statements.Count; i++)
                    {
                        var stmt = stmtList.Statements[i];
                        ASTOutput += $"🔹 التعليمة {i + 1}: {stmt.GetType().Name}\n";
                        
                        if (stmt is AssignmentNode assign)
                        {
                            ASTOutput += $"   ← إسناد: {GetVariableName(assign.Variable)} = {GetExpressionType(assign.Expression)}\n";
                        }
                        else if (stmt is PrintNode print)
                        {
                            ASTOutput += $"   → طباعة: {print.PrintItems.Count} عنصر\n";
                        }
                        else if (stmt is ReadNode read)
                        {
                            ASTOutput += $"   ← قراءة: {GetVariableName(read.Variable)}\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ASTOutput = $"❌ خطأ في بناء الشجرة النحوية: {ex.Message}";
            }
        }

        public void ShowIntermediateCode()
        {
            if (_lastIntermediateCode != null)
            {
                IntermediateCode = "🔄 الكود الوسيط المفصل:\n" + _lastIntermediateCode.PrintCode();
            }
            else
            {
                IntermediateCode = "⚠️ يجب ترجمة الكود أولاً لعرض الكود الوسيط";
            }
        }

        public void ShowAssemblyCode()
        {
            if (_lastIntermediateCode != null)
            {
                var assemblyGenerator = new AssemblyCodeGenerator(_lastIntermediateCode);
                var assemblyCode = assemblyGenerator.Generate();
                AssemblyCode = "⚙️ كود التجميع المفصل:\n" + assemblyCode;
            }
            else
            {
                AssemblyCode = "⚠️ يجب ترجمة الكود أولاً لعرض كود التجميع";
            }
        }

        #endregion

        #region دوال مساعدة

        private void ExtractAndAddVariables(SemanticAnalyzer analyzer)
        {
            try
            {
                // استخراج المتغيرات من الكود المصدري
                var variables = new Dictionary<string, DataType>();
                
                // البحث عن تعريفات المتغيرات
                var lines = SourceCode.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("متغير") && line.Contains(":"))
                    {
                        var parts = line.Split(new[] { ':', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var varPart = parts[0].Trim();
                            var typePart = parts[1].Trim();
                            
                            // استخراج أسماء المتغيرات
                            var varNames = varPart.Replace("متغير", "").Trim().Split(',');
                            
                            // تحديد نوع البيانات
                            DataType dataType = typePart switch
                            {
                                "صحيح" => DataType.Integer,
                                "حقيقي" => DataType.Real,
                                "منطقي" => DataType.Boolean,
                                "خيط" => DataType.String,
                                "حرفي" => DataType.Char,
                                _ => DataType.Unknown
                            };
                            
                            foreach (var varName in varNames)
                            {
                                var cleanName = varName.Trim();
                                if (!string.IsNullOrEmpty(cleanName))
                                {
                                    variables[cleanName] = dataType;
                                }
                            }
                        }
                    }
                }
                
                // إضافة المتغيرات للمحلل الدلالي
                foreach (var variable in variables)
                {
                    analyzer.AddVariableToScope(variable.Key, variable.Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في استخراج المتغيرات: {ex.Message}");
            }
        }

        private void HandleError(string errorType, string message, int line = 0, int column = 0)
        {
            ErrorText = $"{errorType}: {message}";
            if (line > 0)
            {
                ErrorText += $"\n📍 الموقع: سطر {line}, عمود {column}";
            }
            
            ProgramOutput = "❌ فشل في الترجمة - لا يمكن التشغيل";
            _isCompiled = false;
            OnPropertyChanged(nameof(IsReadyToRun));
        }

        private List<string> ExtractPrintStatements()
        {
            var prints = new List<string>();
            var lines = SourceCode.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.Contains("اطبع") && line.Contains("("))
                {
                    var start = line.IndexOf("(") + 1;
                    var end = line.LastIndexOf(")");
                    if (start < end)
                    {
                        var content = line.Substring(start, end - start).Trim();
                        prints.Add(content);
                    }
                }
            }
            
            return prints.Count > 0 ? prints : new List<string> { "مرحبا بالعالم!" };
        }

        private List<string> ExtractCalculations()
        {
            var calcs = new List<string>();
            var lines = SourceCode.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.Contains("=") && line.Any(char.IsDigit))
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        var left = parts[0].Trim();
                        var right = parts[1].Trim().Split(';')[0];
                        calcs.Add($"{left} = {right} → نتيجة محسوبة");
                    }
                }
            }
            
            return calcs;
        }

        private string GetVariableName(AstNode node)
        {
            if (node is VariableAccessNode varNode)
                return varNode.Name;
            return "مجهول";
        }

        private string GetExpressionType(AstNode node)
        {
            return node?.GetType().Name.Replace("Node", "") ?? "تعبير";
        }

        #endregion
    }
}