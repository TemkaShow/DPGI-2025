using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.Windows.Media;

namespace Lab6
{
    public partial class MainWindow : Window
    {
        private bool suppressArabicChanged = false;
        private bool suppressRomanChanged = false;
        private string connectionString;
        private Brush defaultBorderBrush;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ConnectionADO"].ConnectionString;
            defaultBorderBrush = ArabicTextBox.BorderBrush;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConversionHistory();
            ClearTextBoxError(ArabicTextBox);
            ClearTextBoxError(RomanTextBox);
            UpdateSaveButtonState();
        }

        // Встановлює вигляд помилки для TextBox
        private void SetTextBoxError(TextBox textBox, string message)
        {
            textBox.BorderBrush = Brushes.Red;
            textBox.BorderThickness = new Thickness(2);
            textBox.ToolTip = message;
        }

        // Прибирає вигляд помилки з TextBox
        private void ClearTextBoxError(TextBox textBox)
        {
            textBox.BorderBrush = defaultBorderBrush;
            textBox.ClearValue(TextBox.BorderThicknessProperty);
            textBox.ToolTip = null;
        }

        // Оновлює стан активності кнопки "Зберегти"
        private void UpdateSaveButtonState()
        {
            // Кнопка активна, якщо обидва поля заповнені і жодне не має помилки
            bool arabicHasError = ArabicTextBox.BorderBrush == Brushes.Red;
            bool romanHasError = RomanTextBox.BorderBrush == Brushes.Red;

            SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(ArabicTextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(RomanTextBox.Text) &&
                                   !arabicHasError &&
                                   !romanHasError;
        }

        // Завантажує історію конвертацій з бази даних
        private void LoadConversionHistory()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Сортування за Id DESC показує останні додані записи зверху
                    string query = "SELECT Id, ArabicNumeral, RomanNumeral, ConversionDate FROM ConversionHistory ORDER BY Id DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        List<ConversionRecord> conversionRecords = new List<ConversionRecord>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            conversionRecords.Add(new ConversionRecord
                            {
                                Id = Convert.ToInt32(row["Id"]),
                                ArabicNumeral = row["ArabicNumeral"]?.ToString(),
                                RomanNumeral = row["RomanNumeral"]?.ToString(),
                                ConversionDate = row["ConversionDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["ConversionDate"])
                            });
                        }
                        HistoryDataGrid.ItemsSource = conversionRecords;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження історії: " + ex.Message, "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                HistoryDataGrid.ItemsSource = new List<ConversionRecord>();
            }
        }

        // Зберігає поточну конвертацію в історію
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Додаткова перевірка перед збереженням, хоча кнопка має бути неактивною
            if (ArabicTextBox.BorderBrush == Brushes.Red || RomanTextBox.BorderBrush == Brushes.Red ||
                string.IsNullOrWhiteSpace(ArabicTextBox.Text) || string.IsNullOrWhiteSpace(RomanTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть коректні дані в обидва поля.", "Помилка збереження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO ConversionHistory (ArabicNumeral, RomanNumeral) VALUES (@ArabicNumeral, @RomanNumeral)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ArabicNumeral", ArabicTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@RomanNumeral", RomanTextBox.Text.Trim().ToUpper());
                        command.ExecuteNonQuery();
                    }
                }
                LoadConversionHistory();
                MessageBox.Show("Конвертацію успішно збережено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка під час збереження: " + ex.Message, "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateSaveButtonState();
        }

        // Очищує всю історію конвертацій
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirmation = MessageBox.Show(
                "Ви впевнені, що хочете очистити всю історію конвертацій?",
                "Підтвердження очищення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlCommand cmdDelete = new SqlCommand("DELETE FROM ConversionHistory", connection))
                        {
                            cmdDelete.ExecuteNonQuery();
                        }
                        // Скидання лічильника IDENTITY, щоб наступний ID був 1
                        using (SqlCommand cmdReseed = new SqlCommand("DBCC CHECKIDENT ('ConversionHistory', RESEED, 0)", connection))
                        {
                            cmdReseed.ExecuteNonQuery();
                        }
                    }
                    LoadConversionHistory();
                    MessageBox.Show("Історію конвертацій було успішно очищено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка під час очищення історії: " + ex.Message, "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обробник зміни тексту в полі арабського числа
        private void ArabicTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (suppressArabicChanged) return;

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
                    RomanTextBox.Text = ""; // Пусте поле при невалідному діапазоні
                    SetTextBoxError(ArabicTextBox, "Число має бути в діапазоні від 1 до 3999.");
                    ClearTextBoxError(RomanTextBox);
                }
            }
            else // Нечислове введення
            {
                RomanTextBox.Text = "";
                SetTextBoxError(ArabicTextBox, "Введіть дійсне арабське число.");
                ClearTextBoxError(RomanTextBox);
            }
            suppressRomanChanged = false;
            UpdateSaveButtonState();
        }

        // Обробник зміни тексту в полі римського числа
        private void RomanTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (suppressRomanChanged) return;

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
                if (number > 0) // FromRoman повертає 0 для невалідних/неканонічних/поза діапазоном
                {
                    ArabicTextBox.Text = number.ToString();
                    ClearTextBoxError(ArabicTextBox);
                }
                else
                {
                    ArabicTextBox.Text = "";
                    SetTextBoxError(RomanTextBox, "Римське число неваліднe або поза діапазоном (I-MMMCMXCIX).");
                    ClearTextBoxError(ArabicTextBox);
                }
            }
            else // Не відповідає формату римського числа
            {
                ArabicTextBox.Text = "";
                SetTextBoxError(RomanTextBox, "Невірний формат римського числа.");
                ClearTextBoxError(ArabicTextBox);
            }
            suppressArabicChanged = false;
            UpdateSaveButtonState();
        }

        // Конвертує арабське число в римське
        private string ToRoman(int number)
        {
            if (number < 1 || number > 3999) return ""; // Повертає порожній рядок для невалідних

            var romanNumerals = new[]
            {
                (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
                (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
                (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
            };
            StringBuilder result = new StringBuilder();
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

        // Конвертує римське число в арабське
        private int FromRoman(string roman) // Вхідний рядок `roman` має бути ToUpper()
        {
            var romanNumerals = new Dictionary<char, int>
            {
                {'I', 1}, {'V', 5}, {'X', 10}, {'L', 50},
                {'C', 100}, {'D', 500}, {'M', 1000}
            };
            int total = 0;
            int prevValue = 0;
            for (int i = roman.Length - 1; i >= 0; i--)
            {
                int currentValue = romanNumerals[roman[i]];
                if (currentValue < prevValue) total -= currentValue;
                else total += currentValue;
                prevValue = currentValue;
            }
            // Перевірка канонічності та діапазону
            if (total >= 1 && total <= 3999 && ToRoman(total) == roman)
            {
                return total;
            }
            return 0; // Якщо не в діапазоні або не канонічне представлення
        }

        // Перевіряє, чи рядок є валідним римським числом
        private bool IsRoman(string input) // Вхідний рядок `input` має бути ToUpper()
        {
            // Regex для перевірки канонічного формату римських чисел
            return Regex.IsMatch(input, @"^M{0,3}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$");
        }
    }

    public class ConversionRecord
    {
        public int Id { get; set; }
        public string ArabicNumeral { get; set; }
        public string RomanNumeral { get; set; }
        public DateTime ConversionDate { get; set; }
    }
}