using System;
using System.Globalization;
using System.Windows.Data;

namespace Lab7.Commands
{
    // Конвертер для визначення, чи кнопка має бути активною залежно від тексту в TextBox.
    public class TextConverter : IValueConverter
    {
        // Повертає true, якщо значення — непорожній рядок або довжина більше 0.
        // Використовується для прив'язки до IsEnabled.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int length)
                return length > 0;

            if (value is string text)
                return !string.IsNullOrEmpty(text);

            return false;
        }

        // Зворотне перетворення не використовується.

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
