using System;
using System.Threading.Tasks;
using JobAI.Server.Controllers;
using JobAI.Server.Data;
using JobAI.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JobAI.Tests.Integration;

public class VacanciesControllerTests
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

    [Fact]
    public async Task Create_GetById_Delete_ShouldWork()
    {
        var db = CreateDbContext(Guid.NewGuid().ToString());
        var controller = new VacanciesController(db);

        var dto = new VacancyDto
        {
            Title = "Разработчик .NET",
            Company = "Технологии будущего",
            Location = "Москва",
            SourceUrl = "https://example.com/vacancy",
            Description = "Нужен опытный разработчик .NET",
        };

        var created = await controller.Create(dto);
        var createdResult = Assert.IsType<CreatedAtActionResult>(created.Result);
        var createdDto = Assert.IsType<VacancyDto>(createdResult.Value);

        Assert.True(createdDto.Id > 0);
        Assert.NotEqual(default, createdDto.CreatedAt);

        var get = await controller.GetById(createdDto.Id);
        var ok = Assert.IsType<OkObjectResult>(get.Result);
        var getDto = Assert.IsType<VacancyDto>(ok.Value);
        Assert.Equal("Разработчик .NET", getDto.Title);

        var delete = await controller.Delete(createdDto.Id);
        Assert.IsType<NoContentResult>(delete);

        var getAfterDelete = await controller.GetById(createdDto.Id);
        Assert.IsType<NotFoundResult>(getAfterDelete.Result);
    }
}

