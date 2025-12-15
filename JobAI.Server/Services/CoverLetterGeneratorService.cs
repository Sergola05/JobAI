using JobAI.Server.Data;
using JobAI.Server.Models;
using JobAI.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAI.Server.Services
{
    public class CoverLetterGeneratorService
    {
        private readonly JobAiDbContext _db;
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _apiKey;
        private readonly string _model;

        public CoverLetterGeneratorService(
            JobAiDbContext db,
            IConfiguration config,
            IHttpClientFactory httpFactory)
        {
            _db = db;
            _httpFactory = httpFactory;

            _apiKey = config["Perplexity:ApiKey"]
                ?? throw new Exception("Perplexity:ApiKey не задан");

            _model = config["Perplexity:Model"] ?? "sonar-pro";
        }

        // ===== DTO под ответ Perplexity =====

        private class PerplexityResponse
        {
            public List<Choice>? choices { get; set; }
        }

        private class Choice
        {
            public Message? message { get; set; }
        }

        private class Message
        {
            public string? role { get; set; }
            public string? content { get; set; }
        }

        // ===== Основной метод =====

        public async Task<CoverLetter> GenerateAsync(
            GenerateLetterRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var vacancy = await _db.Vacancies
                .FirstOrDefaultAsync(v => v.Id == request.VacancyId, cancellationToken);

            if (vacancy == null)
                throw new Exception("Вакансия не найдена");

            // ---- PROMPT ----
            var prompt = $"""
            Ты — карьерный консультант.
            Сформируй профессиональное сопроводительное письмо на русском языке (200–300 слов).

            Структура:
            1. Вежливое обращение
            2. Краткое представление кандидата
            3. Связь опыта и навыков с вакансией
            4. Мотивация
            5. Заключение

            Вакансия:
            Название: {vacancy.Title}
            Компания: {vacancy.Company}
            Описание: {vacancy.Description}

            Кандидат:
            Имя: {request.CandidateName}
            Контакты: {request.CandidateContacts}
            Навыки: {request.CandidateSkills}
            Опыт: {request.CandidateExperience}

            Верни ТОЛЬКО текст письма.
            """;

            var client = _httpFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await client.PostAsJsonAsync(
                "https://api.perplexity.ai/chat/completions",
                requestBody,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Perplexity API error: {err}");
            }

            var aiResponse = await response.Content
                .ReadFromJsonAsync<PerplexityResponse>(cancellationToken: cancellationToken);

            var text = aiResponse?.choices?.FirstOrDefault()?.message?.content;

            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("Perplexity вернул пустой ответ");

            // ---- Сохранение ----
            var letter = new CoverLetter
            {
                VacancyId = vacancy.Id,
                CandidateName = request.CandidateName,
                CandidateContacts = request.CandidateContacts,
                LetterText = text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.CoverLetters.Add(letter);
            await _db.SaveChangesAsync(cancellationToken);

            return letter;
        }
    }
}
