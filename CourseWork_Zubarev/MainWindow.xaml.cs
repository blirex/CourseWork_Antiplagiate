using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Linq;
using System.Collections.Generic;

namespace CourseWork_Zubarev
{
    public partial class MainWindow : Window
    {
        private bool hasResult = false;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                DatabaseService.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка бази даних: " + ex.Message);
            }
        }


        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog =
                new Microsoft.Win32.OpenFileDialog();

            dialog.Title = "Відкрити документ";
            dialog.Filter =
                "Підтримувані файли (*.txt;*.rtf;*.docx;*.doc;*.pdf;*.odt;*.xlsx;*.xls;*.pptx;*.ppt)|*.txt;*.rtf;*.docx;*.doc;*.pdf;*.odt;*.xlsx;*.xls;*.pptx;*.ppt|" +
                "Усі файли (*.*)|*.*";

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                string text = DocumentReader.ReadText(dialog.FileName);

                ContentTextBox.Text = text;
                TitleTextBox.Text =
                    System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);

                StatusText.Text = "Документ завантажено: " +
                    System.IO.Path.GetFileName(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не вдалося відкрити документ:\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void Check_Click(object sender, RoutedEventArgs e)
        {
            string content = ContentTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Введіть текст або відкрийте файл.");
                return;
            }

            try
            {
                var savedDocuments = DatabaseService.GetDocuments();

                var savedTexts = savedDocuments
                    .Select(document => document.Content)
                    .ToList();

                ComparisonResult result =
                    PlagiarismService.Compare(content, savedTexts);

                ResultText.Text =
                    result.Originality.ToString("F1") + "%";

                StatusText.Text =
                    "Схожість: " + result.Similarity.ToString("F1") + "%\n" +
                    "Спільних унікальних слів: " + result.CommonWords + "\n" +
                    "Документів у базі: " + savedTexts.Count + "\n\n" +
                    "Результат є приблизною оцінкою схожості.";

                hasResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка перевірки: " + ex.Message);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string content = ContentTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show(
                    "Вкажіть назву роботи та її текст.");
                return;
            }

            try
            {
                DatabaseService.SaveDocument(title, content);

                MessageBox.Show(
                    "Роботу збережено в базі даних.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка SQLite",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (!hasResult)
            {
                MessageBox.Show("Спочатку виконайте перевірку.");
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Текстовий звіт (*.txt)|*.txt",
                FileName = "Report.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                string report =
                    "ЗВІТ ПРО ПЕРЕВІРКУ РОБОТИ\n\n" +
                    "Назва: " + TitleTextBox.Text + "\n" +
                    "Дата: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") +
                    "\nОригінальність: " + ResultText.Text + "\n\n" +
                    StatusText.Text;

                try
                {
                    File.WriteAllText(
                        dialog.FileName, report, Encoding.UTF8);

                    MessageBox.Show("Звіт успішно збережено.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Помилка створення звіту: " + ex.Message);
                }
            }
        }

        private void ResetResult()
        {
            hasResult = false;
            ResultText.Text = "—";
            StatusText.Text = "Перевірку ще не виконано.";
        }
    }

}