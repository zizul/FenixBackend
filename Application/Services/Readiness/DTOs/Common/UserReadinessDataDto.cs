using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Services.Readiness.DTOs.Common
{
    public class UserReadinessDataDto
    {
        public ReadinessStatus ReadinessStatus { get; set; }
        public ReadinessRange[] ReadinessRanges { get; set; }
    }
}
