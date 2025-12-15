using JobAI.Server.Data;
using JobAI.Server.Models;
using JobAI.Server.Services;
using JobAI.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobAI.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoverLettersController : ControllerBase
    {
        private readonly JobAiDbContext _db;
        private readonly CoverLetterGeneratorService _generatorService;

        public CoverLettersController(
            JobAiDbContext db,
            CoverLetterGeneratorService generatorService)
        {
            _db = db;
            _generatorService = generatorService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<CoverLetterDto>> Generate([FromBody] GenerateLetterRequestDto request)
        {
            try
            {
                var letter = await _generatorService.GenerateAsync(request);
                
                var vacancy = await _db.Vacancies.FindAsync(letter.VacancyId);
                
                var dto = new CoverLetterDto
                {
                    Id = letter.Id,
                    VacancyId = letter.VacancyId,
                    VacancyTitle = vacancy?.Title ?? string.Empty,
                    CandidateName = letter.CandidateName,
                    CandidateContacts = letter.CandidateContacts,
                    LetterText = letter.LetterText,
                    CreatedAt = letter.CreatedAt,
                    UpdatedAt = letter.UpdatedAt
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("by-vacancy/{vacancyId}")]
        public async Task<ActionResult<IEnumerable<CoverLetterDto>>> GetByVacancy(int vacancyId)
        {
            var letters = await _db.CoverLetters
                .Include(l => l.Vacancy)
                .Where(l => l.VacancyId == vacancyId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var result = letters.Select(l => new CoverLetterDto
            {
                Id = l.Id,
                VacancyId = l.VacancyId,
                VacancyTitle = l.Vacancy?.Title ?? string.Empty,
                CandidateName = l.CandidateName,
                CandidateContacts = l.CandidateContacts,
                LetterText = l.LetterText,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] CoverLetterDto dto)
        {
            var letter = await _db.CoverLetters.FindAsync(id);
            if (letter == null)
                return NotFound();

            letter.CandidateName = dto.CandidateName;
            letter.CandidateContacts = dto.CandidateContacts;
            letter.LetterText = dto.LetterText;
            letter.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
