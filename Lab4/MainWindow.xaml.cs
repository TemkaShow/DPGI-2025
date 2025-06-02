using System;
using System.Data;
using System.Windows;

namespace Lab4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Створюємо екземпляр нашого класу для доступу до даних
        private AdoAssistant adoAssistant;
        private DataTable clientsData = new DataTable();

        public MainWindow()
        {
            InitializeComponent();
            adoAssistant = new AdoAssistant(); // Ініціалізуємо клас AdoAssistant
        }

        // Обробник події, який спрацьовує при завантаженні вікна
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData(); // Завантажуємо дані при старті програми
        }

        // Метод для завантаження або оновлення даних у ListBox
        private void LoadData()
        {
            clientsData = adoAssistant.GetClients(); // Отримуємо дані з БД

            // Перевіряємо, чи дані успішно завантажені
            if (clientsData != null)
            {
                ClientListBox.DataContext = clientsData.DefaultView;
            }
            else
            {
                ClientListBox.DataContext = null;
                MessageBox.Show("Не вдалося завантажити дані клієнтів.");
            }
        }

        // Обробник для створення нового клієнта
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Відкриваємо діалогове вікно для введення даних нового клієнта
                WindowClient dialog = new WindowClient();
                dialog.Owner = this; // Встановлюємо власника вікна

                if (dialog.ShowDialog() == true)
                {
                    // Додаємо клієнта в базу даних
                    bool success = adoAssistant.AddClient(
                        dialog.ClientName,
                        dialog.ClientPhone,
                        dialog.ClientAddress,
                        dialog.ClientOrderAmount
                    );

                    if (success)
                    {
                        MessageBox.Show("Клієнт успішно доданий!", "Інформація",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData(); // Оновлюємо дані у списку
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при створенні клієнта: {ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник для оновлення вибраного клієнта
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Перевіряємо, чи вибрано запис
                if (ClientListBox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, виберіть клієнта для оновлення.",
                        "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Отримуємо вибраний рядок
                DataRowView selectedRow = (DataRowView)ClientListBox.SelectedItem;
                int clientId = Convert.ToInt32(selectedRow["Id"]);

                // Отримуємо значення з текстових полів
                string name = NameTextBox.Text;
                string phone = PhoneTextBox.Text;
                string address = AddressTextBox.Text;

                // Виправлений код парсингу для уникнення проблем з форматом числа
                decimal orderAmount;

                // Спробуємо очистити введений текст від можливих нечислових символів
                string cleanedInput = OrderAmountTextBox.Text.Replace(",", ".");

                if (!decimal.TryParse(cleanedInput,
                                     System.Globalization.NumberStyles.Any,
                                     System.Globalization.CultureInfo.InvariantCulture,
                                     out orderAmount))
                {
                    MessageBox.Show("Некоректне значення суми замовлення. Введіть числове значення.",
                                   "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Оновлюємо дані в базі
                bool success = adoAssistant.UpdateClient(clientId, name, phone, address, orderAmount);

                if (success)
                {
                    MessageBox.Show("Дані клієнта успішно оновлені!", "Інформація",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData(); // Оновлюємо дані у списку
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні даних клієнта: {ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник для видалення вибраного клієнта
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Перевіряємо, чи вибрано запис
                if (ClientListBox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, виберіть клієнта для видалення.",
                        "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Отримуємо ID вибраного клієнта
                DataRowView selectedRow = (DataRowView)ClientListBox.SelectedItem;
                int clientId = Convert.ToInt32(selectedRow["Id"]);
                string clientName = selectedRow["Name"].ToString();

                // Підтвердження видалення
                MessageBoxResult result = MessageBox.Show(
                    $"Ви дійсно бажаєте видалити клієнта \"{clientName}\"?",
                    "Підтвердження видалення",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Видаляємо запис з бази даних
                    bool success = adoAssistant.DeleteClient(clientId);

                    if (success)
                    {
                        MessageBox.Show("Клієнт успішно видалений!", "Інформація",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData(); // Оновлюємо дані у списку
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при видаленні клієнта: {ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}