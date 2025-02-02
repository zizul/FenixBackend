namespace Domain.ValueObjects
{
    public class Address
    {
        public string? Street { get; }
        public string? House { get; }
        public string? Flat { get; }
        public string? Floor { get; }
        public string? PostalCode { get; }
        public string? Town { get; }


        public Address(
            string? street,
            string? house = null,
            string? flat = null,
            string? floor = null,
            string? postalCode = null,
            string? town = null
            )
        {
            this.Street = street;
            this.House = house;
            this.Flat = flat;
            this.Floor = floor;
            this.PostalCode = postalCode;
            this.Town = town;
        }
    }
}