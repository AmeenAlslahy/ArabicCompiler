using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System;
using ICSharpCode.AvalonEdit.Document;

namespace ArabicCompilerIDE
{
    public partial class MainWindow : Window
    {
        private string _currentFilePath;
        private bool _isFileModified = false;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
            
            // تهيئة المحرر
            InitializeEditor();
            
            // تسجيل الأحداث
            SourceCodeEditor.TextChanged += SourceCodeEditor_TextChanged;
            SourceCodeEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            
            // تهيئة الواجهة
            UpdateLineInfo();
            UpdateFileInfo();
            UpdateTitle();
            
            // تعيين اختصارات لوحة المفاتيح
            SetKeyboardShortcuts();
        }

        private void InitializeEditor()
        {
            try
            {
                // تعيين التلوين النحوي
                if (TryFindResource("ArabicHighlighting") is ICSharpCode.AvalonEdit.Highlighting.HighlightingDefinition highlighting)
                {
                    SourceCodeEditor.SyntaxHighlighting = highlighting;
                }
                
                // تعيين خيارات المحرر
                SourceCodeEditor.ShowLineNumbers = true;
                SourceCodeEditor.Options.EnableHyperlinks = true;
                SourceCodeEditor.Options.EnableEmailHyperlinks = true;
                SourceCodeEditor.Options.HighlightCurrentLine = true;
                SourceCodeEditor.Options.CutCopyWholeLine = true;
                
                // ربط النص مع ViewModel
                if (DataContext is MainViewModel viewModel)
                {
                    SourceCodeEditor.Document = new TextDocument(viewModel.SourceCode);
                    SourceCodeEditor.Document.TextChanged += (s, e) =>
                    {
                        viewModel.SourceCode = SourceCodeEditor.Text;
                        _isFileModified = true;
                        UpdateTitle();
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تهيئة المحرر: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            UpdateLineInfo();
        }

        private void SourceCodeEditor_TextChanged(object sender, EventArgs e)
        {
            _isFileModified = true;
            UpdateTitle();
        }

        #region تحديثات الواجهة

        private void UpdateLineInfo()
        {
            try
            {
                if (SourceCodeEditor != null)
                {
                    var caret = SourceCodeEditor.TextArea.Caret;
                    LineInfo.Text = $"السطر: {caret.Line}, العمود: {caret.Column}";
                }
            }
            catch
            {
                LineInfo.Text = "السطر: 1, العمود: 1";
            }
        }

        private void UpdateFileInfo()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                FileInfo.Text = "غير محفوظ";
            }
            else
            {
                FileInfo.Text = Path.GetFileName(_currentFilePath);
            }
        }

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(_currentFilePath) ? "غير محفوظ" : Path.GetFileName(_currentFilePath);
            string modified = _isFileModified ? " *" : "";
            this.Title = $"{fileName}{modified} - مترجم اللغة العربية - Arabic Compiler IDE";
        }

        private void SetKeyboardShortcuts()
        {
            // F5 - تشغيل
            var runBinding = new KeyBinding(
                new RelayCommand(_ => RunButton_Click(null, null)),
                Key.F5,
                ModifierKeys.None);
            this.InputBindings.Add(runBinding);

            // F7 - ترجمة
            var compileBinding = new KeyBinding(
                new RelayCommand(_ => CompileButton_Click(null, null)),
                Key.F7,
                ModifierKeys.None);
            this.InputBindings.Add(compileBinding);
        }

        #endregion

        #region أحداث القوائم والأزرار

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "جاري التشغيل...";
            
            if (this.DataContext is MainViewModel viewModel)
            {
                await viewModel.ExecuteRunAsync();
            }
            
            StatusText.Text = "تم التشغيل بنجاح";
        }

        private async void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "جاري الترجمة...";
            
            if (this.DataContext is MainViewModel viewModel)
            {
                await viewModel.ExecuteCompileAsync();
            }
            
            StatusText.Text = "تم الترجمة بنجاح";
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            if (_isFileModified)
            {
                var result = MessageBox.Show("هل تريد حفظ التغييرات قبل إنشاء ملف جديد؟", 
                    "تغييرات غير محفوظة", 
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile_Click(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.NewFile();
                _currentFilePath = null;
                _isFileModified = false;
                UpdateFileInfo();
                UpdateTitle();
                StatusText.Text = "تم إنشاء ملف جديد";
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (_isFileModified)
            {
                var result = MessageBox.Show("هل تريد حفظ التغييرات قبل فتح ملف آخر؟", 
                    "تغييرات غير محفوظة", 
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile_Click(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "ملفات اللغة العربية (*.arabic)|*.arabic|ملفات النص (*.txt)|*.txt|جميع الملفات (*.*)|*.*",
                Title = "فتح ملف كود مصدري",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    if (this.DataContext is MainViewModel viewModel)
                    {
                        viewModel.SourceCode = fileContent;
                        SourceCodeEditor.Document.Text = fileContent;
                        _currentFilePath = openFileDialog.FileName;
                        _isFileModified = false;
                        UpdateFileInfo();
                        UpdateTitle();
                        StatusText.Text = $"تم فتح الملف: {Path.GetFileName(openFileDialog.FileName)}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في فتح الملف: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "خطأ في فتح الملف";
                }
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SaveFileAs();
            }
            else
            {
                SaveToFile(_currentFilePath);
            }
        }

        private void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
        }

        private void SaveFileAs()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "ملفات اللغة العربية (*.arabic)|*.arabic|ملفات النص (*.txt)|*.txt|جميع الملفات (*.*)|*.*",
                Title = "حفظ الكود المصدري",
                DefaultExt = ".arabic",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _currentFilePath = saveFileDialog.FileName;
                SaveToFile(_currentFilePath);
            }
        }

        private void SaveToFile(string filePath)
        {
            try
            {
                if (this.DataContext is MainViewModel viewModel)
                {
                    File.WriteAllText(filePath, viewModel.SourceCode);
                    _isFileModified = false;
                    UpdateFileInfo();
                    UpdateTitle();
                    StatusText.Text = $"تم حفظ الملف: {Path.GetFileName(filePath)}";
                    
                    MessageBox.Show("تم حفظ الملف بنجاح", "حفظ", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ الملف: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "خطأ في حفظ الملف";
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (_isFileModified)
            {
                var result = MessageBox.Show("هل تريد حفظ التغييرات قبل الخروج؟", 
                    "تغييرات غير محفوظة", 
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile_Click(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            Application.Current.Shutdown();
        }

        #endregion

        #region أحداث عرض المخرجات

        private void ShowTokens_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.ShowTokens();
                OutputTabControl.SelectedIndex = 0; // تبويب الرموز
                StatusText.Text = "عرض الرموز اللغوية";
            }
        }

        private void ShowAST_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.ShowAST();
                OutputTabControl.SelectedIndex = 1; // تبويب الشجرة
                StatusText.Text = "عرض الشجرة النحوية";
            }
        }

        private void ShowIntermediate_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.ShowIntermediateCode();
                OutputTabControl.SelectedIndex = 2; // تبويب الوسيط
                StatusText.Text = "عرض الكود الوسيط";
            }
        }

        private void ShowAssembly_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.ShowAssemblyCode();
                OutputTabControl.SelectedIndex = 3; // تبويب التجميع
                StatusText.Text = "عرض كود التجميع";
            }
        }

        private void ToggleOutput_Click(object sender, RoutedEventArgs e)
        {
            // تبديل عرض نافذة الإخراج
            var outputTab = FindTabItem("📤 الإخراج");
            if (outputTab != null)
            {
                outputTab.Visibility = outputTab.Visibility == Visibility.Visible ? 
                    Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void ToggleErrors_Click(object sender, RoutedEventArgs e)
        {
            // تبديل عرض نافذة الأخطاء
            var errorsTab = FindTabItem("⚠️ الأخطاء");
            if (errorsTab != null)
            {
                errorsTab.Visibility = errorsTab.Visibility == Visibility.Visible ? 
                    Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void ToggleLineNumbers_Click(object sender, RoutedEventArgs e)
        {
            if (SourceCodeEditor != null)
            {
                SourceCodeEditor.ShowLineNumbers = !SourceCodeEditor.ShowLineNumbers;
            }
        }

        private void ToggleRuler_Click(object sender, RoutedEventArgs e)
        {
            // إضافة/إزالة المسطرة إذا كانت مدعومة
        }

        private void ClearInput_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.UserInput = "";
            }
        }

        private void SendInput_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "تم إرسال بيانات الإدخال";
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("دليل المستخدم:\n\n" +
                "• F5: تشغيل البرنامج\n" +
                "• F7: ترجمة الكود\n" +
                "• Ctrl+S: حفظ الملف\n" +
                "• Ctrl+O: فتح ملف\n" +
                "• Ctrl+N: ملف جديد", 
                "دليل المستخدم", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("مترجم اللغة العربية - Arabic Compiler IDE\n\n" +
                "إصدار 1.0\n" +
                "مشروع مترجم للغة العربية البرمجية\n" +
                "تم التطوير باستخدام WPF و .NET 6", 
                "عن البرنامج", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        private TabItem FindTabItem(string header)
        {
            if (OutputTabControl != null)
            {
                foreach (TabItem tabItem in OutputTabControl.Items)
                {
                    if (tabItem.Header.ToString() == header)
                    {
                        return tabItem;
                    }
                }
            }
            return null;
        }

        #endregion

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_isFileModified)
            {
                var result = MessageBox.Show("هل تريد حفظ التغييرات قبل الخروج؟", 
                    "تغييرات غير محفوظة", 
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile_Click(null, null);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }
    }
}