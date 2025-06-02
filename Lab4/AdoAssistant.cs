using System;
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

        // Додавання нового клієнта в базу даних
        // Альтернативний метод додавання клієнта з генерацією Id
        public bool AddClient(string name, string phone, string address, decimal orderAmount)
        {
            bool success = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Знаходимо максимальне значення Id
                string getMaxIdQuery = "SELECT ISNULL(MAX([Id]), 0) FROM [dbo].[Clients]";
                SqlCommand getMaxIdCommand = new SqlCommand(getMaxIdQuery, connection);
                int maxId = 0;

                try
                {
                    maxId = Convert.ToInt32(getMaxIdCommand.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при отриманні максимального Id: {ex.Message}");
                    return false;
                }

                // Генеруємо новий Id (максимальний + 1)
                int newId = maxId + 1;

                // Вставляємо нового клієнта з вказаним Id
                string insertQuery = "INSERT INTO [dbo].[Clients] ([Id], [Name], [Phone], [Address], [Order amount]) " +
                                   "VALUES (@Id, @Name, @Phone, @Address, @OrderAmount)";

                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@Id", newId);
                insertCommand.Parameters.AddWithValue("@Name", name);
                insertCommand.Parameters.AddWithValue("@Phone", phone);
                insertCommand.Parameters.AddWithValue("@Address", address);
                insertCommand.Parameters.AddWithValue("@OrderAmount", orderAmount);

                try
                {
                    int result = insertCommand.ExecuteNonQuery();
                    success = result > 0;

                    // Очищаємо кеш, щоб при наступному запиті отримати оновлені дані
                    if (success)
                        ClearCache();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"SQL помилка під час додавання клієнта: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка під час додавання клієнта: {ex.Message}");
                }
            }

            return success;
        }

        // Оновлення існуючого клієнта в базі даних
        public bool UpdateClient(int id, string name, string phone, string address, decimal orderAmount)
        {
            bool success = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE [dbo].[Clients] " +
                               "SET [Name] = @Name, [Phone] = @Phone, [Address] = @Address, [Order amount] = @OrderAmount " +
                               "WHERE [Id] = @Id";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Address", address);
                command.Parameters.AddWithValue("@OrderAmount", orderAmount);

                try
                {
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    success = result > 0;

                    // Очищаємо кеш, щоб при наступному запиті отримати оновлені дані
                    if (success)
                        ClearCache();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"SQL помилка під час оновлення клієнта: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка під час оновлення клієнта: {ex.Message}");
                }
            }

            return success;
        }

        // Видалення клієнта з бази даних
        public bool DeleteClient(int id)
        {
            bool success = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM [dbo].[Clients] WHERE [Id] = @Id";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    success = result > 0;

                    // Очищаємо кеш, щоб при наступному запиті отримати оновлені дані
                    if (success)
                        ClearCache();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"SQL помилка під час видалення клієнта: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка під час видалення клієнта: {ex.Message}");
                }
            }

            return success;
        }
    }
}