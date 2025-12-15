namespace JobAI.Server.Models
{
    public class CoverLetter
    {
        public int Id { get; set; }

 
        public int VacancyId { get; set; }
        public Vacancy Vacancy { get; set; } = null!;

        public string CandidateName { get; set; } = string.Empty;
        public string CandidateContacts { get; set; } = string.Empty;
        public string LetterText { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
