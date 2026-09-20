using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CreateEnrolmentDto
    {
        [Required]
        public int CategoryId { get; set; }
    }
}