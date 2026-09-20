using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CreateResultDto
    {
        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int FinishPosition { get; set; }
    }
}