using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects
{
    public record CompanyForCreationDto
    {

        [Required(ErrorMessage = "Company name is a required field.")]
        [MaxLength(60, ErrorMessage = "Maximum length for the Name is 60 characters.")]
        public required string Name { get; set; }
        [Required(ErrorMessage = "Company address is a required field.")]
        [MaxLength(60, ErrorMessage = "Maximum length for the Address is 60 characters")]
        public required string Address { get; set; }
        [Required(ErrorMessage = "Company Country  is a required field.")]
        public required string Country { get; set; }
        public IEnumerable<EmployeeForCreationDto>? Employees { get; set; }
    }
}
