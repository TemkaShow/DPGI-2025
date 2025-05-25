using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Lab7
{
    public partial class PageConverter : Page
    {
        private bool suppressArabicChanged = false;
        private bool suppressRomanChanged = false;
        private string connectionString;
        private Brush defaultBorderBrush;

        public PageConverter()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ConnectionADO"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Рядок підключення 'ConnectionADO' не знайдено або порожній в App.config.\nЗбереження в історію буде неможливим.",
                    "Помилка конфігурації", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ArabicTextBox != null)
            {
                defaultBorderBrush = ArabicTextBox.BorderBrush;
                ClearTextBoxError(ArabicTextBox);
                ClearTextBoxError(RomanTextBox);
                ConverterTitleTextBlock.Text = "Конвертер чисел";
            }
        }

        // Command Handlers
        private void UndoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (ArabicTextBox != null && !string.IsNullOrWhiteSpace(ArabicTextBox.Text)) ||
                           (RomanTextBox != null && !string.IsNullOrWhiteSpace(RomanTextBox.Text));
        }

        private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ArabicTextBox.Text = "";
            RomanTextBox.Text = "";
            ClearTextBoxError(ArabicTextBox);
            ClearTextBoxError(RomanTextBox);
            CommandManager.InvalidateRequerySuggested();
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (ArabicTextBox != null && !string.IsNullOrWhiteSpace(ArabicTextBox.Text)) ||
                          (RomanTextBox != null && !string.IsNullOrWhiteSpace(RomanTextBox.Text));
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ArabicTextBox.Text = "";
            RomanTextBox.Text = "";
            ClearTextBoxError(ArabicTextBox);
            ClearTextBoxError(RomanTextBox);
            ConverterTitleTextBlock.Text = "Конвертер чисел";
            CommandManager.InvalidateRequerySuggested();
        }

        private void EditCommand_CanExecute_NotApplicable(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ArabicTextBox == null || RomanTextBox == null) { e.CanExecute = false; return; }
            bool arabicHasError = ArabicTextBox.BorderBrush == Brushes.Red;
            bool romanHasError = RomanTextBox.BorderBrush == Brushes.Red;
            e.CanExecute = !string.IsNullOrWhiteSpace(ArabicTextBox.Text) &&
                           !string.IsNullOrWhiteSpace(RomanTextBox.Text) &&
                           !arabicHasError && !romanHasError &&
                           !string.IsNullOrEmpty(connectionString);
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Неможливо зберегти: проблеми з рядком підключення до БД.",
                    "Помилка збереження", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ArabicTextBox.BorderBrush == Brushes.Red || RomanTextBox.BorderBrush == Brushes.Red ||
                string.IsNullOrWhiteSpace(ArabicTextBox.Text) || string.IsNullOrWhiteSpace(RomanTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть коректні дані в обидва поля.",
                    "Помилка збереження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO ConversionHistory (ArabicNumeral, RomanNumeral, ConversionDate) VALUES (@ArabicNumeral, @RomanNumeral, GETDATE())";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ArabicNumeral", ArabicTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@RomanNumeral", RomanTextBox.Text.Trim().ToUpper());
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Конвертацію успішно збережено в історію.",
                    "Збережено", MessageBoxButton.OK, MessageBoxImage.Information);
                ArabicTextBox.Text = "";
                RomanTextBox.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка під час збереження в історію: " + ex.Message,
                    "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        }

        // Helper Methods
        private void SetTextBoxError(TextBox textBox, string message)
        {
            textBox.BorderBrush = Brushes.Red;
            textBox.BorderThickness = new Thickness(1.5);
            ToolTipService.SetToolTip(textBox, message);
        }

        private void ClearTextBoxError(TextBox textBox)
        {
            if (textBox == null) return;
            textBox.BorderBrush = defaultBorderBrush;
            textBox.ClearValue(TextBox.BorderThicknessProperty);
            ToolTipService.SetToolTip(textBox, null);
        }

        private string ToRoman(int number)
        {
            if (number < 1 || number > 3999) return "";
            var romanNumerals = new[] {
                (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
                (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
                (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
            };
            var result = new StringBuilder();
            foreach (var (value, numeral) in romanNumerals)
            {
                while (number >= value)
                {
                    result.Append(numeral);
                    number -= value;
                }
            }
            return result.ToString();
        }

        private int FromRoman(string roman)
        {
            var romanMap = new Dictionary<char, int>
            {
                { 'I', 1 }, { 'V', 5 }, { 'X', 10 }, { 'L', 50 },
                { 'C', 100 }, { 'D', 500 }, { 'M', 1000 }
            };
            int total = 0, previous = 0;
            for (int i = roman.Length - 1; i >= 0; i--)
            {
                if (!romanMap.ContainsKey(roman[i])) return 0;
                int current = romanMap[roman[i]];
                total += current < previous ? -current : current;
                previous = current;
            }
            return (total >= 1 && total <= 3999 && ToRoman(total) == roman) ? total : 0;
        }

        private bool IsRoman(string input) =>
            Regex.IsMatch(input, @"^M{0,3}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$");

        // Text Changed Events
        private void ArabicTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (suppressArabicChanged || ArabicTextBox == null || RomanTextBox == null) return;
            string input = ArabicTextBox.Text.Trim();
            ClearTextBoxError(ArabicTextBox);
            suppressRomanChanged = true;

            if (string.IsNullOrWhiteSpace(input))
            {
                RomanTextBox.Text = "";
                ClearTextBoxError(RomanTextBox);
            }
            else if (int.TryParse(input, out int number))
            {
                if (number >= 1 && number <= 3999)
                {
                    RomanTextBox.Text = ToRoman(number);
                    ClearTextBoxError(RomanTextBox);
                }
                else
                {
                    RomanTextBox.Text = "";
                    SetTextBoxError(ArabicTextBox, "Число: 1-3999.");
                    ClearTextBoxError(RomanTextBox);
                }
            }
            else
            {
                RomanTextBox.Text = "";
                SetTextBoxError(ArabicTextBox, "Введіть дійсне арабське число.");
                ClearTextBoxError(RomanTextBox);
            }

            suppressRomanChanged = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void RomanTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (suppressRomanChanged || ArabicTextBox == null || RomanTextBox == null) return;
            string input = RomanTextBox.Text.Trim().ToUpper();
            ClearTextBoxError(RomanTextBox);
            suppressArabicChanged = true;

            if (string.IsNullOrWhiteSpace(input))
            {
                ArabicTextBox.Text = "";
                ClearTextBoxError(ArabicTextBox);
            }
            else if (IsRoman(input))
            {
                int number = FromRoman(input);
                if (number > 0)
                {
                    ArabicTextBox.Text = number.ToString();
                    ClearTextBoxError(ArabicTextBox);
                }
                else
                {
                    ArabicTextBox.Text = "";
                    SetTextBoxError(RomanTextBox, "Римське: I-MMMCMXCIX, канонічне.");
                    ClearTextBoxError(ArabicTextBox);
                }
            }
            else
            {
                ArabicTextBox.Text = "";
                SetTextBoxError(RomanTextBox, "Невірний формат римського числа.");
                ClearTextBoxError(ArabicTextBox);
            }

            suppressArabicChanged = false;
            CommandManager.InvalidateRequerySuggested();
        }

        // Navigation Event Handlers
        private void GoToMainPage_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null)
            {
                try
                {
                    this.NavigationService.Navigate(new Uri("PageMain.xaml", UriKind.Relative));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка навігації на головну сторінку: {ex.Message}",
                        "Помилка навігації", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GoToHistoryPage_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null)
            {
                try
                {
                    this.NavigationService.Navigate(new Uri("PageHistory.xaml", UriKind.Relative));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка навігації на сторінку історії: {ex.Message}",
                        "Помилка навігації", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}