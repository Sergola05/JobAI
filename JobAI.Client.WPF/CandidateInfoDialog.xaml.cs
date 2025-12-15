using System.Windows;
using JobAI.Client.WPF.Models;

namespace JobAI.Client.WPF
{
    public partial class CandidateInfoDialog : Window
    {
        public CandidateInfoDialog()
        {
            InitializeComponent();
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || 
                string.IsNullOrWhiteSpace(ContactsTextBox.Text))
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

        public GenerateLetterRequestDto GetRequest()
        {
            return new GenerateLetterRequestDto
            {
                CandidateName = NameTextBox.Text.Trim(),
                CandidateContacts = ContactsTextBox.Text.Trim(),
                CandidateSkills = SkillsTextBox.Text.Trim(),
                CandidateExperience = ExperienceTextBox.Text.Trim()
            };
        }
    }
}


