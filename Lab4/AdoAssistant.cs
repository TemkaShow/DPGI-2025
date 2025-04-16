using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace Lab4
{
    public class AdoAssistant
    {
        // Отримуємо рядок з'єднання з файлу App.config
        private readonly string connectionString = ConfigurationManager
            .ConnectionStrings["connectionString_ADO"].ConnectionString;

        // Об'єкт DataTable для кешування даних клієнтів
        private DataTable dtClients;

        // Метод очищає кеш даних, змушуючи наступні запити завантажувати дані з бази даних
        public void ClearCache()
        {
            this.dtClients = null;
        }

        // Метод повертає DataTable, що містить усіх клієнтів з бази даних
        public DataTable GetClients()
        {
            // Повернути кешовані дані, якщо вони доступні
            if (dtClients != null && dtClients.Rows.Count > 0)
                return dtClients;

            // Створити нову таблицю, якщо вона не кешована
            dtClients = new DataTable();

            // Використання 'using' гарантує автоматичне закриття та звільнення ресурсів
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // SQL-запит з явно вказаними колонками
                string query = "SELECT [Id], [Name], [Phone], [Address], [Order amount] FROM [dbo].[Clients]";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);

                try
                {
                    // Заповнюємо DataTable даними
                    adapter.Fill(dtClients);
                }
                catch (SqlException ex)
                {
                    // Специфічна обробка SQL помилок
                    MessageBox.Show($"SQL помилка під час завантаження клієнтів: {ex.Message}");
                    dtClients = new DataTable(); // Повернути порожню таблицю замість null
                }
                catch (Exception ex)
                {
                    // Обробка загальних помилок
                    MessageBox.Show($"Помилка під час завантаження даних клієнтів: {ex.Message}");
                    dtClients = new DataTable();
                }
            }

            return dtClients;
        }
    }
}