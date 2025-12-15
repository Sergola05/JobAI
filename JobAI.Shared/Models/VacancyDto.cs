using System;

namespace JobAI.Shared.Models
{
    /// <summary>
    /// DTO-вариант вакансии для обмена между клиентом и сервером.
    /// </summary>
    public class VacancyDto
    {
        public int Id { get; set; }                     // Id в нашей БД
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
