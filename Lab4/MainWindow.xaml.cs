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
    }
}