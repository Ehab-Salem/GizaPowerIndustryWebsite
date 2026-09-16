// Models/VacancyApplicationViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GizaPowerIndustryWebsite.Models
{
    public class VacancyApplicationViewModel
    {
        public int VacancyId { get; set; }

        // Display-only, populated by the GET action
        public string JobTitle { get; set; }
        public string DepartmentName { get; set; }

        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your address.")]
        [StringLength(100)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please enter your phone number.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(50)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Range(0, 60, ErrorMessage = "Enter years of experience between 0 and 60.")]
        [Display(Name = "Years of Experience")]
        public int Experience { get; set; }

        [Required]
        [Range(0, 99999999, ErrorMessage = "Enter a valid expected salary.")]
        [Display(Name = "Expected Salary")]
        public decimal ExpectedSalary { get; set; }

        [Required(ErrorMessage = "Please enter your current salary.")]
        [StringLength(200)]
        [Display(Name = "Current Salary")]
        public string CurrentSalary { get; set; }

        [Required(ErrorMessage = "Please attach your CV.")]
        [Display(Name = "Upload CV (PDF or Word, max 5MB)")]
        public IFormFile CvFile { get; set; }
    }
}