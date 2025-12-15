namespace JobAI.Client.WPF.Models
{
    public class GenerateLetterRequestDto
    {
        public int VacancyId { get; set; }
        public string CandidateName { get; set; } = "";
        public string CandidateContacts { get; set; } = "";
        public string CandidateSkills { get; set; } = "";
        public string CandidateExperience { get; set; } = "";
    }
}
