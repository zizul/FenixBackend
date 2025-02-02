namespace Domain.ValueObjects
{
    public class Coordinates
    {
        public double Longitude { get; }
        public double Latitude { get; }


        public Coordinates(double longitude, double latitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        }

        public static bool operator ==(Coordinates a, Coordinates b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Longitude == b.Longitude && a.Latitude == b.Latitude;
        }

        public static bool operator !=(Coordinates a, Coordinates b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is Coordinates other && this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Longitude, Latitude);
        }
    }
}