using Domain.ValueObjects;

namespace Domain.Entities.Map
{
    public abstract class PointOfInterest
    {
        public int Id { get; set; }
        public Coordinates Coordinates { get; set; }
    }
}