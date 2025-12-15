namespace JobAI.Server.Models
{
    public class Vacancy
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Company { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string SourceUrl { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<CoverLetter> CoverLetters { get; set; } = new List<CoverLetter>();
    }
}

