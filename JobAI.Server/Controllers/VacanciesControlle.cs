using JobAI.Server.Data;
using JobAI.Server.Models;
using JobAI.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobAI.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VacanciesController : ControllerBase
    {
        private readonly JobAiDbContext _db;

        public VacanciesController(JobAiDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VacancyDto>>> GetAll()
        {
            var vacancies = await _db.Vacancies
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var result = vacancies.Select(v => new VacancyDto
            {
                Id = v.Id,
                Title = v.Title,
                Company = v.Company,
                Location = v.Location,
                SourceUrl = v.SourceUrl,
                Description = v.Description,
                CreatedAt = v.CreatedAt
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VacancyDto>> GetById(int id)
        {
            var v = await _db.Vacancies.FindAsync(id);
            if (v == null)
                return NotFound();

            var dto = new VacancyDto
            {
                Id = v.Id,
                Title = v.Title,
                Company = v.Company,
                Location = v.Location,
                SourceUrl = v.SourceUrl,
                Description = v.Description,
                CreatedAt = v.CreatedAt
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<VacancyDto>> Create([FromBody] VacancyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.Company))
            {
                return BadRequest("Не заполнены обязательные поля Title или Company.");
            }

            var entity = new Vacancy
            {
                Title = dto.Title,
                Company = dto.Company,
                Location = dto.Location,
                SourceUrl = dto.SourceUrl,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _db.Vacancies.Add(entity);
            await _db.SaveChangesAsync();

            dto.Id = entity.Id;
            dto.CreatedAt = entity.CreatedAt;

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var v = await _db.Vacancies.FindAsync(id);
            if (v == null)
                return NotFound();

            _db.Vacancies.Remove(v);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}

