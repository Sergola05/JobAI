using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using JobAI.Server.Controllers;
using JobAI.Server.Data;
using JobAI.Server.Models;
using JobAI.Server.Services;
using JobAI.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace JobAI.Tests.Integration;

public class CoverLettersControllerTests
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
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Perplexity:ApiKey"] = apiKey,
                ["Perplexity:Model"] = model,
            }!)
            .Build();
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public FakeHttpClientFactory(HttpClient client) => _client = client;

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
    public async Task Generate_ReturnsDto_AndSavesToDb()
    {
        var dbName = Guid.NewGuid().ToString();
        var db = CreateDbContext(dbName);
        var config = CreateConfig();

        var vacancy = new Vacancy
        {
            Id = 1,
            Title = "Разработчик .NET",
            Company = "Технологии будущего",
            Location = "Москва",
            SourceUrl = "https://example.com/vacancy",
            Description = "Нужен опытный разработчик .NET",
            CreatedAt = DateTime.UtcNow
        };
        db.Vacancies.Add(vacancy);
        await db.SaveChangesAsync();

        var expectedText = "Письмо от сервиса (для теста).";
        var fakePerplexityJson = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new { message = new { role = "assistant", content = expectedText } }
            }
        });

        var httpClient = new HttpClient(new FakeHttpMessageHandler(fakePerplexityJson));
        var httpFactory = new FakeHttpClientFactory(httpClient);

        var generator = new CoverLetterGeneratorService(db, config, httpFactory);
        var controller = new CoverLettersController(db, generator);

        var request = new GenerateLetterRequestDto
        {
            VacancyId = 1,
            CandidateName = "Иван Иванов",
            CandidateContacts = "ivan@example.com",
            CandidateSkills = "C#, ASP.NET Core",
            CandidateExperience = "5 лет"
        };

        var action = await controller.Generate(request);
        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var dto = Assert.IsType<CoverLetterDto>(ok.Value);

        Assert.Equal(1, dto.VacancyId);
        Assert.Equal("Иван Иванов", dto.CandidateName);
        Assert.Equal(expectedText, dto.LetterText);

        // Проверяем сохранение.
        var saved = await db.CoverLetters.Include(l => l.Vacancy).SingleAsync(l => l.Id == dto.Id);
        Assert.Equal(expectedText, saved.LetterText);
        Assert.Equal(vacancy.Id, saved.VacancyId);
    }

    [Fact]
    public async Task Update_ShouldChangeLetterText_AndSetUpdatedAt()
    {
        var db = CreateDbContext(Guid.NewGuid().ToString());

        var vacancy = new Vacancy
        {
            Id = 1,
            Title = "Разработчик .NET",
            Company = "Технологии будущего",
            Location = "Москва",
            SourceUrl = "https://example.com/vacancy",
            Description = "Нужен опытный разработчик .NET",
            CreatedAt = DateTime.UtcNow
        };

        var letter = new CoverLetter
        {
            Id = 1,
            VacancyId = 1,
            Vacancy = vacancy,
            CandidateName = "Старое имя",
            CandidateContacts = "old@example.com",
            LetterText = "Старый текст",
            CreatedAt = DateTime.UtcNow
        };

        db.Vacancies.Add(vacancy);
        db.CoverLetters.Add(letter);
        await db.SaveChangesAsync();

        var requestDto = new CoverLetterDto
        {
            VacancyId = 1,
            CandidateName = "Новое имя",
            CandidateContacts = "new@example.com",
            LetterText = "Новый текст"
        };

        // генератор не участвует в Update
        var controller = new CoverLettersController(db,
            generatorService: new CoverLetterGeneratorService(
                db,
                CreateConfig(),
                new FakeHttpClientFactory(new HttpClient(new FakeHttpMessageHandler("{}")))));

        var result = await controller.Update(1, requestDto);
        Assert.IsType<NoContentResult>(result);

        var updated = await db.CoverLetters.SingleAsync(l => l.Id == 1);
        Assert.Equal("Новое имя", updated.CandidateName);
        Assert.Equal("new@example.com", updated.CandidateContacts);
        Assert.Equal("Новый текст", updated.LetterText);
        Assert.True(updated.UpdatedAt.HasValue);
        Assert.True(updated.UpdatedAt.Value <= DateTime.UtcNow);
    }
}

