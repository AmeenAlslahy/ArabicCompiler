using System.Windows;
using System.Windows.Input;
using ArabicCompiler; // لاستخدام مكونات المترجم

namespace ArabicCompilerIDE
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }

    public class MainViewModel : ViewModelBase
    {
        private string _sourceCode = "برنامج مثال ;\n{\n    متغير x : صحيح ;\n    x = 10 + 5 ;\n    اطبع ( x ) ;\n} .";
        private string _intermediateCode = "الكود الوسيط سيظهر هنا...";
        private string _assemblyCode = "كود التجميع سيظهر هنا...";
        private string _errorText = "الأخطاء والتحذيرات ستظهر هنا...";

        public string SourceCode
        {
            get => _sourceCode;
            set => SetProperty(ref _sourceCode, value);
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

        public ICommand CompileCommand { get; }

        public MainViewModel()
        {
            CompileCommand = new RelayCommand(ExecuteCompile);
        }

        private void ExecuteCompile(object parameter)
        {
            try
            {
                // 1. Lexer
                var lexer = new Lexer(SourceCode);
                
                // 2. Parser
                var parser = new Parser(lexer);
                var ast = parser.ParseProgram();
                
                // 3. Semantic Analyzer
                var semanticAnalyzer = new SemanticAnalyzer();
                // يجب إضافة تعريفات المتغيرات هنا بشكل صحيح، ولكن للتبسيط سنضيفها يدوياً
                semanticAnalyzer.AddVariableToScope("x", DataType.Integer);
                
                semanticAnalyzer.Analyze(ast);

                // 4. Code Generator
                var codeGenerator = new CodeGenerator(semanticAnalyzer);
                var intermediateCode = codeGenerator.Generate(ast);

                // 5. Code Generation (Assembly)
                var assemblyGenerator = new AssemblyCodeGenerator(intermediateCode);
                var assemblyCode = assemblyGenerator.Generate();

                         // 5. Code Generation (Assembly)
                var assemblyGenerator = new AssemblyCodeGenerator(intermediateCode);
                var assemblyCode = assemblyGenerator.Generate();

                // عرض النتائج
                IntermediateCode = intermediateCode.PrintCode();
                AssemblyCode = assemblyCode;
                ErrorText = "الترجمة تمت بنجاح. لا توجد أخطاء.";ء.";
            }
            catch (LexerException ex)
            {
                ErrorText = $"خطأ لغوي: {ex.Message}";
                IntermediateCode = "فشل في الترجمة.";
                AssemblyCode = "فشل في الترجمة.";
            }
            catch (ParserException ex)
            {
                ErrorText = $"خطأ نحوي: {ex.Message}";
                IntermediateCode = "فشل في الترجمة.";
                AssemblyCode = "فشل في الترجمة.";
            }
            catch (SemanticException ex)
            {
                ErrorText = $"خطأ دلالي: {ex.Message}";
                IntermediateCode = "فشل في الترجمة.";
                AssemblyCode = "فشل في الترجمة.";
            }
            catch (Exception ex)
            {
                ErrorText = $"خطأ عام: {ex.Message}";
                IntermediateCode = "فشل في الترجمة.";
                AssemblyCode = "فشل في الترجمة.";
            }
        }
    }

    // فئة أساسية لتطبيق نمط INotifyPropertyChanged
    public class ViewModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    // فئة لتطبيق نمط ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
