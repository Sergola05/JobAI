using JobAI.Server.Data;
using JobAI.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("JobAiDatabase");

builder.Services.AddDbContext<JobAiDbContext>(options =>
    options.UseSqlServer(connectionString));

// HttpClient для работы с внешними API
builder.Services.AddHttpClient();

// наш сервис генерации писем
builder.Services.AddScoped<CoverLetterGeneratorService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
