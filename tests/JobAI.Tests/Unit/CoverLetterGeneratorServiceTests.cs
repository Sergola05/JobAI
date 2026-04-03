using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using JobAI.Server.Data;
using JobAI.Server.Models;
using JobAI.Server.Services;
using JobAI.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace JobAI.Tests.Unit;

public class CoverLetterGeneratorServiceTests
{
    private static JobAiDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<JobAiDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var db = new JobAiDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private static IConfiguration CreateConfig(string apiKey = "test-api-key", string model = "sonar-pro")
    {
        var dict = new Dictionary<string, string?>
        {
            ["Perplexity:ApiKey"] = apiKey,
            ["Perplexity:Model"] = model,
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict!)
            .Build();
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public FakeHttpClientFactory(HttpClient client) => _client = client;

        // Некоторые версии кода могут вызывать CreateClient() без имени.
        public HttpClient CreateClient() => _client;

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _json;
        private readonly HttpStatusCode _statusCode;

        public FakeHttpMessageHandler(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _json = json;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task GenerateAsync_WhenVacancyNotFound_ShouldThrow()
    {
        var db = CreateDbContext(Guid.NewGuid().ToString());
        var config = CreateConfig();

        // В этом сценарии внешний API не вызывается, вакансия отсутствует.
        var handler = new FakeHttpMessageHandler("{\"choices\":[]}");
        var httpClient = new HttpClient(handler);
        var httpFactory = new FakeHttpClientFactory(httpClient);

        var service = new CoverLetterGeneratorService(db, config, httpFactory);

        var request = new GenerateLetterRequestDto
        {
            VacancyId = 1,
            CandidateName = "Иван",
            CandidateContacts = "ivan@example.com",
            CandidateSkills = "C#",
            CandidateExperience = "3 года"
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => service.GenerateAsync(request));
        Assert.Contains("Вакансия не найдена", ex.Message);
    }

    [Fact]
    public async Task GenerateAsync_WhenApiReturnsText_ShouldSaveCoverLetter()
    {
        var dbName = Guid.NewGuid().ToString();
        var db = CreateDbContext(dbName);
        var config = CreateConfig();

        db.Vacancies.Add(new Vacancy
        {
            Id = 1,
            Title = "Разработчик .NET",
            Company = "Технологии будущего",
            Location = "Москва",
            SourceUrl = "https://example.com/vacancy",
            Description = "Нужен опытный разработчик .NET",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var expectedText = "Это сгенерированное сопроводительное письмо.";
        var fakePerplexityJson = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new { role = "assistant", content = expectedText }
                }
            }
        });

        var handler = new FakeHttpMessageHandler(fakePerplexityJson);
        var httpClient = new HttpClient(handler);
        var httpFactory = new FakeHttpClientFactory(httpClient);

        var service = new CoverLetterGeneratorService(db, config, httpFactory);

        var request = new GenerateLetterRequestDto
        {
            VacancyId = 1,
            CandidateName = "Иван Иванов",
            CandidateContacts = "ivan@example.com",
            CandidateSkills = "C#, ASP.NET Core",
            CandidateExperience = "5 лет"
        };

        var letter = await service.GenerateAsync(request);

        Assert.NotNull(letter);
        Assert.Equal(1, letter.VacancyId);
        Assert.Equal("Иван Иванов", letter.CandidateName);
        Assert.Equal(expectedText, letter.LetterText);
        Assert.True(letter.CreatedAt <= DateTime.UtcNow);

        // Проверяем, что письмо реально сохранилось в БД.
        var saved = await db.CoverLetters.SingleAsync(l => l.Id == letter.Id);
        Assert.Equal(expectedText, saved.LetterText);
    }
}

