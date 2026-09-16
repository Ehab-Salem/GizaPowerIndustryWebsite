namespace GizaPowerIndustryWebsite.Models
{
    public class CareersViewModel
    {
        public List<DepartmentTabViewModel> Departments { get; set; } = new();
        public List<VacancyCardViewModel> Vacancies { get; set; } = new();
    }

    public class DepartmentTabViewModel
    {
        public string Name { get; set; }
        public string Slug { get; set; }
    }

    public class VacancyCardViewModel
    {
        public int Id { get; set; }
        public string JobTitle { get; set; }
        public string JobSummary { get; set; }
        public string Department { get; set; }
        public string DepartmentSlug { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
        public string SeniorityLevel { get; set; }
    }

    // ---- Job details page ----
    public class VacancyDetailsViewModel
    {
        public int Id { get; set; }
        public string JobTitle { get; set; }
        public string JobSummary { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
        public string SeniorityLevel { get; set; }

        public List<string> Responsibilities { get; set; } = new();
        public List<string> Requirements { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public List<VacancyLanguageDisplay> Languages { get; set; } = new();

        public List<RelatedJobViewModel> RelatedJobs { get; set; } = new();
    }

    public class VacancyLanguageDisplay
    {
        public string Language { get; set; }
        public string Level { get; set; }
    }

    public class RelatedJobViewModel
    {
        public int Id { get; set; }
        public string JobTitle { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
    }
}
