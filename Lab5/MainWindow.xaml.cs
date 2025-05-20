using Lab5.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace Lab5
{
    public partial class MainWindow : Window
    {
        private ClientsEntities dbContext = new ClientsEntities();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllData();
        }

        private void LoadAllData()
        {
            try
            {
                // --- Вкладка 1: Клієнти ---
                ClientsDataGrid.ItemsSource = dbContext.Clients
                    .Select(c => new
                    {
                        c.ClientID,
                        c.ClientName,
                        c.Phone,
                        CompanyName = c.Companies != null ? c.Companies.CompanyName : "(без компанії)",
                        c.Income,
                        c.Expenses
                    })
                    .ToList();

                // --- Вкладка 2: Компанії ---
                var companies = dbContext.Companies
                    .Select(co => new
                    {
                        co.CompanyCode,
                        co.CompanyName
                    })
                    .ToList();
                CompaniesDataGrid.ItemsSource = companies;

                // --- Вкладка 3: Кількість клієнтів по компаніях ---
                var clientsCountByCompany = dbContext.Clients
                   .Where(c => c.CompanyCode != null)
                   .GroupBy(c => c.CompanyCode)
                   .Select(g => new
                   {
                       CompanyName = dbContext.Companies
                                         .Where(co => co.CompanyCode == g.Key)
                                         .Select(co => co.CompanyName)
                                         .FirstOrDefault() ?? "(невідома компанія)",
                       NumberOfClients = g.Count()
                   })
                   .ToList();
                ClientsCountByCompanyDataGrid.ItemsSource = clientsCountByCompany;

                CompanyFilterComboBox.ItemsSource = companies;
                CompanyFilterComboBox.DisplayMemberPath = "CompanyName";
                CompanyFilterComboBox.SelectedValuePath = "CompanyCode";

                if (companies.Any())
                {
                    CompanyFilterComboBox.SelectedItem = null;
                    ClientsByCompanyFilterDataGrid.ItemsSource = new List<object>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}\nInnerException: {ex.InnerException?.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Вкладка 4: Обробник кнопки "Пошук клієнта" за ім'ям ---
        private void ClientNameSearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                string searchText = ClientNameSearchTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(searchText))
                {
                    FilteredClientsDataGrid.ItemsSource = dbContext.Clients
                        .Where(c => c.ClientName.ToLower().Contains(searchText.ToLower()))
                        .Select(c => new
                        {
                            c.ClientID,
                            c.ClientName,
                            c.Phone,
                            CompanyName = c.Companies != null ? c.Companies.CompanyName : "(без компанії)",
                            c.Income,
                            c.Expenses
                        })
                        .ToList();
                }
                else
                {
                    FilteredClientsDataGrid.ItemsSource = new List<object>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка пошуку: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // --- Вкладка 5: Пошук клієнтів за обраною компанією ---
        private void CompanyFilterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (CompanyFilterComboBox.SelectedValue != null)
                {
                    int selectedCompanyId = (int)CompanyFilterComboBox.SelectedValue;

                    ClientsByCompanyFilterDataGrid.ItemsSource = dbContext.Clients
                        .Where(c => c.CompanyCode == selectedCompanyId)
                        .Select(c => new
                        {
                            c.ClientID,
                            c.ClientName,
                            c.Phone,
                            CompanyName = c.Companies != null ? c.Companies.CompanyName : "(без компанії)",
                            c.Income,
                            c.Expenses
                        })
                        .ToList();
                }
                else
                {
                    ClientsByCompanyFilterDataGrid.ItemsSource = new List<object>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка фільтрації: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closed_EventHandler(object sender, EventArgs e)
        {
            dbContext?.Dispose();
        }

    }
}