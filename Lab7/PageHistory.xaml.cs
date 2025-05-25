using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Lab7.Commands;

namespace Lab7
{
    public partial class PageHistory : Page
    {
        private string connectionString;
        private List<ConversionRecord> _fullConversionHistory;
        private ConversionRecord _currentlySelectedItem;
        private bool _isEditMode = false;
        private ObservableCollection<ConversionRecord> _editableCollection;

        public PageHistory()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ConnectionADO"]?.ConnectionString;
            _fullConversionHistory = new List<ConversionRecord>();
            _editableCollection = new ObservableCollection<ConversionRecord>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Рядок підключення 'ConnectionADO' не знайдено або порожній. Історія не буде завантажена.",
                    "Помилка конфігурації", MessageBoxButton.OK, MessageBoxImage.Error);
                HistoryDataGrid.ItemsSource = new ObservableCollection<ConversionRecord>();
                return;
            }
            LoadConversionHistory();
        }

        private void LoadConversionHistory()
        {
            if (string.IsNullOrEmpty(connectionString)) return;

            _fullConversionHistory.Clear();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, ArabicNumeral, RomanNumeral, ConversionDate FROM ConversionHistory ORDER BY Id DESC";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        foreach (DataRow row in dataTable.Rows)
                        {
                            _fullConversionHistory.Add(new ConversionRecord
                            {
                                Id = Convert.ToInt32(row["Id"]),
                                ArabicNumeral = row["ArabicNumeral"]?.ToString(),
                                RomanNumeral = row["RomanNumeral"]?.ToString(),
                                ConversionDate = row["ConversionDate"] == DBNull.Value
                                    ? DateTime.MinValue
                                    : Convert.ToDateTime(row["ConversionDate"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження історії: " + ex.Message,
                    "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                _fullConversionHistory = new List<ConversionRecord>();
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (HistoryDataGrid == null) return;

            string searchText = SearchHistoryTextBox?.Text.Trim().ToUpper() ?? "";
            var sourceToDisplay = _fullConversionHistory;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                sourceToDisplay = _fullConversionHistory
                    .Where(record => (record.ArabicNumeral != null && record.ArabicNumeral.ToUpper().Contains(searchText)) ||
                                     (record.RomanNumeral != null && record.RomanNumeral.ToUpper().Contains(searchText)))
                    .ToList();
            }

            if (_isEditMode)
            {
                _editableCollection.Clear();
                foreach (var record in sourceToDisplay)
                {
                    _editableCollection.Add(record);
                }
                HistoryDataGrid.ItemsSource = _editableCollection;
            }
            else
            {
                HistoryDataGrid.ItemsSource = new ObservableCollection<ConversionRecord>(sourceToDisplay);
            }

            if (_currentlySelectedItem != null && sourceToDisplay.Contains(_currentlySelectedItem))
            {
                HistoryDataGrid.SelectedItem = _currentlySelectedItem;
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void SearchHistoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ClearSearchHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            SearchHistoryTextBox.Text = "";
        }

        private void HistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentlySelectedItem = HistoryDataGrid.SelectedItem as ConversionRecord;
            CommandManager.InvalidateRequerySuggested();
        }

        // Command Handlers
        private void UndoSelectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _currentlySelectedItem != null || _isEditMode;
        }

        private void UndoSelectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_isEditMode)
            {
                // Скасування редагування
                MessageBoxResult result = MessageBox.Show("Ви впевнені, що хочете скасувати редагування? Всі незбережені зміни будуть втрачені.",
                    "Скасування редагування", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _isEditMode = false;
                    HistoryDataGrid.IsReadOnly = true;
                    LoadConversionHistory(); // Перезавантажуємо оригінальні дані
                    CommandManager.InvalidateRequerySuggested();

                    MessageBox.Show("Режим редагування скасовано.", "Інформація",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (HistoryDataGrid != null)
            {
                // Скасування вибору
                HistoryDataGrid.SelectedItem = null;
            }
        }

        private void FindHistoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SearchHistoryTextBox != null;
        }

        private void FindHistoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SearchHistoryTextBox?.Focus();
        }

        private void DeleteHistoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool canInteractWithDb = !string.IsNullOrEmpty(connectionString);
            bool historyHasItems = _fullConversionHistory != null && _fullConversionHistory.Any();
            bool itemIsSelected = _currentlySelectedItem != null;

            e.CanExecute = canInteractWithDb && (itemIsSelected || historyHasItems) && !_isEditMode;
        }

        private void DeleteHistoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Неможливо видалити: проблеми з рядком підключення до БД.",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_currentlySelectedItem != null)
            {
                ConversionRecord recordToDelete = _currentlySelectedItem;
                MessageBoxResult confirmation = MessageBox.Show(
                    $"Ви впевнені, що хочете видалити запис ID={recordToDelete.Id} ({recordToDelete.ArabicNumeral} <-> {recordToDelete.RomanNumeral})?",
                    "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirmation == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string deleteQuery = "DELETE FROM ConversionHistory WHERE Id = @Id";
                            using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@Id", recordToDelete.Id);
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    _fullConversionHistory.Remove(recordToDelete);
                                    ApplyFilter();
                                    MessageBox.Show("Запис успішно видалено.", "Видалення",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("Не вдалося знайти запис для видалення в БД (можливо, його вже видалено іншим шляхом).",
                                        "Помилка видалення", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    LoadConversionHistory();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка під час видалення запису: " + ex.Message,
                            "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else if (_fullConversionHistory != null && _fullConversionHistory.Any())
            {
                MessageBoxResult confirmation = MessageBox.Show(
                    "Не обрано жодного запису. Ви хочете очистити всю історію конвертацій?",
                    "Підтвердження очищення історії", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirmation == MessageBoxResult.Yes)
                {
                    ClearAllHistory();
                }
            }
            else
            {
                MessageBox.Show("Немає записів для видалення.", "Видалення",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Edit Command Handlers
        private void EditHistoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool canInteractWithDb = !string.IsNullOrEmpty(connectionString);
            bool historyHasItems = _fullConversionHistory != null && _fullConversionHistory.Any();

            e.CanExecute = canInteractWithDb && historyHasItems && !_isEditMode;
        }

        private void EditHistoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _isEditMode = true;
            HistoryDataGrid.IsReadOnly = false;
            ApplyFilter();
            CommandManager.InvalidateRequerySuggested();

            MessageBox.Show("Режим редагування активовано. Ви можете редагувати арабські та римські числа.\nНе забудьте зберегти зміни!",
                "Режим редагування", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveHistoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _isEditMode && !string.IsNullOrEmpty(connectionString);
        }

        private void SaveHistoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_isEditMode || _editableCollection == null) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var record in _editableCollection)
                    {
                        string updateQuery = "UPDATE ConversionHistory SET ArabicNumeral = @ArabicNumeral, RomanNumeral = @RomanNumeral WHERE Id = @Id";
                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Id", record.Id);
                            command.Parameters.AddWithValue("@ArabicNumeral", record.ArabicNumeral ?? "");
                            command.Parameters.AddWithValue("@RomanNumeral", record.RomanNumeral ?? "");
                            command.ExecuteNonQuery();
                        }
                    }
                }

                // Вихід з режиму редагування
                _isEditMode = false;
                HistoryDataGrid.IsReadOnly = true;

                // Перезавантаження даних з бази
                LoadConversionHistory();

                CommandManager.InvalidateRequerySuggested();

                MessageBox.Show("Зміни успішно збережено!", "Збереження",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка під час збереження змін: " + ex.Message,
                    "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAllHistory()
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
                    using (SqlCommand cmdReseed = new SqlCommand("DBCC CHECKIDENT ('ConversionHistory', RESEED, 0)", connection))
                    {
                        cmdReseed.ExecuteNonQuery();
                    }
                }
                _fullConversionHistory.Clear();
                _currentlySelectedItem = null;
                ApplyFilter();
                MessageBox.Show("Історію конвертацій було успішно очищено.", "Інформація",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка під час очищення історії: " + ex.Message,
                    "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Navigation Event Handlers
        private void GoToMainPage_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode)
            {
                MessageBoxResult result = MessageBox.Show("Ви знаходитесь в режимі редагування. Зберегти зміни перед переходом?",
                    "Незбережені зміни", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveHistoryCommand_Executed(sender, null);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            if (NavigationService != null)
                NavigationService.Navigate(new Uri("PageMain.xaml", UriKind.Relative));
        }

        private void GoToConverterPage_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode)
            {
                MessageBoxResult result = MessageBox.Show("Ви знаходитесь в режимі редагування. Зберегти зміни перед переходом?",
                    "Незбережені зміни", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveHistoryCommand_Executed(sender, null);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            if (NavigationService != null)
                NavigationService.Navigate(new Uri("PageConverter.xaml", UriKind.Relative));
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