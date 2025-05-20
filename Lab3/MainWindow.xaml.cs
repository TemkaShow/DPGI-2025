using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Lab3;

public partial class MainWindow : Window
{
    private bool suppressArabicChanged = false;
    private bool suppressRomanChanged = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ArabicTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (suppressArabicChanged) return;

        string input = ArabicTextBox.Text.Trim();
        if (int.TryParse(input, out int number))
        {
            if (number >= 1 && number <= 3999)
            {
                suppressRomanChanged = true;
                RomanTextBox.Text = ToRoman(number);
                suppressRomanChanged = false;
            }
            else
            {
                suppressRomanChanged = true;
                RomanTextBox.Text = "!";
                suppressRomanChanged = false;
            }
        }
        else
        {
            suppressRomanChanged = true;
            RomanTextBox.Text = "";
            suppressRomanChanged = false;
        }
    }

    private void RomanTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (suppressRomanChanged) return;

        string input = RomanTextBox.Text.Trim().ToUpper();
        if (IsRoman(input))
        {
            int number = FromRoman(input);
            suppressArabicChanged = true;
            ArabicTextBox.Text = number.ToString();
            suppressArabicChanged = false;
        }
        else
        {
            suppressArabicChanged = true;
            ArabicTextBox.Text = "";
            suppressArabicChanged = false;
        }
    }

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
            if (!romanNumerals.ContainsKey(c)) return 0;

            int currentValue = romanNumerals[c];
            total += currentValue;

            if (currentValue > prevValue)
                total -= 2 * prevValue;

            prevValue = currentValue;
        }
        return total;
    }

    private bool IsRoman(string input)
    {
        return Regex.IsMatch(input,
            "^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$");
    }
}
