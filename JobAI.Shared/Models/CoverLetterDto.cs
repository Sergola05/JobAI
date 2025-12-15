using System;

namespace JobAI.Shared.Models
{
    /// <summary>
    /// DTO сопроводительного письма.
    /// </summary>
    public class CoverLetterDto
    {
        public int Id { get; set; }

        public int VacancyId { get; set; }
        public string VacancyTitle { get; set; } = string.Empty;

        public string CandidateName { get; set; } = string.Empty;
        public string CandidateContacts { get; set; } = string.Empty;

        public string LetterText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
