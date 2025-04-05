using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab3;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Обробка натискання кнопки
    private void ConvertButton_Click(object sender, RoutedEventArgs e)
    {
        ConvertInput();
    }

    // Обробка клавіші Enter
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConvertInput();
            e.Handled = true;
        }
    }

    // Основна логіка обробки введених даних
    private void ConvertInput()
    {
        string input = InputTextBox.Text.Trim();

        // Перевірка на порожнє поле
        if (string.IsNullOrEmpty(input))
        {
            MessageBox.Show("Будь ласка, введіть число для конвертації.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Якщо це арабське число
        if (int.TryParse(input, out int arabicNumber))
        {
            if (arabicNumber < 1 || arabicNumber > 3999)
            {
                MessageBox.Show("Арабське число має бути від 1 до 3999.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultTextBlock.Text = $"Римське: {ToRoman(arabicNumber)}";
        }
        // Якщо це римське число
        else if (IsRoman(input))
        {
            int converted = FromRoman(input);
            ResultTextBlock.Text = $"Арабське: {converted}";
        }
        // Якщо введення некоректне
        else
        {
            MessageBox.Show("Введення не є ні римським, ні арабським числом.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Метод для конвертації з арабських чисел у римські
    private string ToRoman(int number)
    {
        var romanNumerals = new (int, string)[]
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

    // Метод для конвертації з римських чисел в арабські
    private int FromRoman(string roman)
    {
        var romanNumerals = new Dictionary<char, int>
            {
                {'I', 1}, {'V', 5}, {'X', 10}, {'L', 50},
                {'C', 100}, {'D', 500}, {'M', 1000}
            };

        int total = 0;
        int prevValue = 0;

        foreach (char c in roman.ToUpper())
        {
            int currentValue = romanNumerals[c];
            total += currentValue;

            // Віднімання в римських числах (наприклад IV = 5 - 1)
            if (currentValue > prevValue)
                total -= 2 * prevValue;

            prevValue = currentValue;
        }
        return total;
    }

    // Перевірка, чи введення є римським числом
    private bool IsRoman(string input)
    {
        return Regex.IsMatch(input.ToUpper(),
            "^M*(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$");
    }


}