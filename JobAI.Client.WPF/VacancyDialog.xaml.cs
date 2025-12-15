using System.Windows;
using JobAI.Shared.Models;

namespace JobAI.Client.WPF
{
    public partial class VacancyDialog : Window
    {
        public VacancyDialog()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) || 
                string.IsNullOrWhiteSpace(CompanyTextBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Заполните все обязательные поля (отмечены *)", 
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public VacancyDto GetVacancy()
        {
            return new VacancyDto
            {
                Title = TitleTextBox.Text.Trim(),
                Company = CompanyTextBox.Text.Trim(),
                Location = LocationTextBox.Text.Trim(),
                SourceUrl = SourceUrlTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text.Trim()
            };
        }
    }
}


