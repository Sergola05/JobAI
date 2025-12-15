using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JobAI.Client.WPF.Services;
using JobAI.Shared.Models;
using VacancyDto = JobAI.Shared.Models.VacancyDto;
using CoverLetterDto = JobAI.Shared.Models.CoverLetterDto;
using GenerateLetterRequestDto = JobAI.Client.WPF.Models.GenerateLetterRequestDto;

namespace JobAI.Client.WPF
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient;
        private List<VacancyDto> _vacancies = new List<VacancyDto>();
        private List<CoverLetterDto> _letters = new List<CoverLetterDto>();
        private int? _selectedVacancyId = null;

        public MainWindow()
        {
            InitializeComponent();
            _apiClient = new ApiClient();
            LoadVacancies();
        }

        private async void LoadVacancies()
        {
            try
            {
                StatusTextBlock.Text = "Загрузка вакансий...";
                _vacancies = await _apiClient.GetVacanciesAsync();
                VacanciesListBox.ItemsSource = _vacancies;
                StatusTextBlock.Text = $"Загружено вакансий: {_vacancies.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке вакансий: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка загрузки";
            }
        }

        private async void DeleteVacancy_Click(object sender, RoutedEventArgs e)
        {
            if (VacanciesListBox.SelectedItem is VacancyDto vacancy)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить вакансию '{vacancy.Title}'?\nВсе связанные письма также будут удалены.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await DeleteVacancy(vacancy.Id);
                }
            }
        }

        private async Task DeleteVacancy(int vacancyId)
        {
            try
            {
                StatusTextBlock.Text = "Удаление вакансии...";
                await _apiClient.DeleteVacancyAsync(vacancyId);
                StatusTextBlock.Text = "Вакансия удалена";
                LoadVacancies();
                LettersListBox.ItemsSource = null;
                LetterTextBox.Text = "Выберите вакансию";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении вакансии: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка удаления";
            }
        }

        private void AddVacancy_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VacancyDialog();
            if (dialog.ShowDialog() == true)
            {
                var vacancy = dialog.GetVacancy();
                CreateVacancy(vacancy);
            }
        }

        private async void CreateVacancy(VacancyDto vacancy)
        {
            try
            {
                StatusTextBlock.Text = "Создание вакансии...";
                await _apiClient.CreateVacancyAsync(vacancy);
                StatusTextBlock.Text = "Вакансия создана";
                LoadVacancies();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании вакансии: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка создания";
            }
        }

        private async void VacanciesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VacanciesListBox.SelectedItem is VacancyDto vacancy)
            {
                _selectedVacancyId = vacancy.Id;
                GenerateButton.IsEnabled = true;
                DeleteVacancyButton.IsEnabled = true;
                await LoadLetters(vacancy.Id);
            }
            else
            {
                _selectedVacancyId = null;
                GenerateButton.IsEnabled = false;
                DeleteVacancyButton.IsEnabled = false;
                LettersListBox.ItemsSource = null;
                LetterTextBox.Text = "Выберите вакансию";
                LetterTextBox.IsReadOnly = true;
                SaveLetterButton.IsEnabled = false;
            }
        }

        private async Task LoadLetters(int vacancyId)
        {
            try
            {
                StatusTextBlock.Text = "Загрузка писем...";
                _letters = await _apiClient.GetLettersByVacancyAsync(vacancyId);
                LettersListBox.ItemsSource = _letters;
                StatusTextBlock.Text = $"Загружено писем: {_letters.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке писем: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка загрузки";
            }
        }

        private void LettersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LettersListBox.SelectedItem is CoverLetterDto letter)
            {
                LetterTextBox.Text = letter.LetterText;
                LetterTextBox.IsReadOnly = false;
                SaveLetterButton.IsEnabled = true;
                _currentLetter = letter;
            }
            else
            {
                LetterTextBox.Text = "Выберите письмо для просмотра";
                LetterTextBox.IsReadOnly = true;
                SaveLetterButton.IsEnabled = false;
                _currentLetter = null;
            }
        }

        private CoverLetterDto _currentLetter = null;

        private async void SaveLetter_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLetter == null)
            {
                MessageBox.Show("Выберите письмо для редактирования", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _currentLetter.LetterText = LetterTextBox.Text;
                StatusTextBlock.Text = "Сохранение изменений...";
                await _apiClient.UpdateLetterAsync(_currentLetter);
                StatusTextBlock.Text = "Изменения сохранены";
                MessageBox.Show("Письмо успешно обновлено!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                if (_selectedVacancyId.HasValue)
                {
                    await LoadLetters(_selectedVacancyId.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении письма: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка сохранения";
            }
        }

        private void GenerateLetter_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedVacancyId.HasValue)
            {
                MessageBox.Show("Выберите вакансию", "Внимание", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new CandidateInfoDialog();
            if (dialog.ShowDialog() == true)
            {
                var request = dialog.GetRequest();
                request.VacancyId = _selectedVacancyId.Value;
                GenerateLetter(request);
            }
        }

        private async void GenerateLetter(GenerateLetterRequestDto request)
        {
            try
            {
                GenerateButton.IsEnabled = false;
                StatusTextBlock.Text = "Генерация письма...";
                
                var letter = await _apiClient.GenerateLetterAsync(request);
                
                StatusTextBlock.Text = "Письмо успешно сгенерировано";
                MessageBox.Show("Письмо успешно сгенерировано!", "Успех", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                if (_selectedVacancyId.HasValue)
                {
                    await LoadLetters(_selectedVacancyId.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации письма: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка генерации";
            }
            finally
            {
                GenerateButton.IsEnabled = true;
            }
        }
    }
}
