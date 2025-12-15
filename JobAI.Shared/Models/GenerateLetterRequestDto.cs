namespace JobAI.Shared.Models
{
    /// <summary>
    /// Запрос на генерацию сопроводительного письма.
    /// </summary>
    public class GenerateLetterRequestDto
    {
        public int VacancyId { get; set; }

        public string CandidateName { get; set; } = string.Empty;
        public string CandidateContacts { get; set; } = string.Empty;
        public string CandidateSkills { get; set; } = string.Empty;
        public string CandidateExperience { get; set; } = string.Empty;
    }
}

