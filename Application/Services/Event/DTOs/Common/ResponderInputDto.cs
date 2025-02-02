using Domain.Enums;

namespace Application.Services.Event.DTOs.Common
{
    public class ResponderInputDto
    {
        public ResponderStatusType? Status { get; set; }
        public DateTime? ETA { get; set; }
        public TransportType? Transport { get; set; }
    }
}
