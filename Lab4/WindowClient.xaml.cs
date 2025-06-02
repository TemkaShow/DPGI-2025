using System;
using System.Windows;

namespace Lab4
{
    /// <summary>
    /// Interaction logic for WindowClient.xaml
    /// </summary>
    public partial class WindowClient : Window
    {
        // Властивості для збереження введених даних
        public string ClientName { get; private set; }
        public string ClientPhone { get; private set; }
        public string ClientAddress { get; private set; }
        public decimal ClientOrderAmount { get; private set; }

        // Конструктор за замовчуванням
        public WindowClient()
        {
            InitializeComponent();
        }

        // Конструктор для режиму редагування клієнта
        public WindowClient(string name, string phone, string address, decimal orderAmount)
        {
            InitializeComponent();

            // Заповнюємо поля існуючими даними
            NameTextBox.Text = name;
            PhoneTextBox.Text = phone;
            AddressTextBox.Text = address;
            OrderAmountTextBox.Text = orderAmount.ToString();

            // Змінюємо заголовок вікна
            Title = "Редагувати клієнта";
        }

        // Обробник натискання кнопки OK
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Перевірка коректності введених даних
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть ім'я клієнта!", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            // Виправлене парсингу суми замовлення
            decimal orderAmount;
            string cleanedInput = OrderAmountTextBox.Text.Replace(",", ".");

            if (!decimal.TryParse(cleanedInput,
                                 System.Globalization.NumberStyles.Any,
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 out orderAmount))
            {
                MessageBox.Show("Некоректне значення суми замовлення! Введіть числове значення.",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                OrderAmountTextBox.Focus();
                return;
            }

            // Зберігаємо введені дані в публічні властивості
            ClientName = NameTextBox.Text;
            ClientPhone = PhoneTextBox.Text;
            ClientAddress = AddressTextBox.Text;
            ClientOrderAmount = orderAmount;

            // Закриваємо діалог з результатом true (OK)
            DialogResult = true;
        }

        // Обробник натискання кнопки Скасувати
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Закриваємо діалог з результатом false (Cancel)
            DialogResult = false;
        }
    }
}