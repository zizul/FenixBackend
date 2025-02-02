using Domain.Entities.User;
using Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Persistance.Repositories.User.Serialization
{
    internal class UserConverter : JsonConverter<BasicUser>
    {
        public override BasicUser? ReadJson(JsonReader reader, Type objectType, BasicUser? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            dynamic json = JObject.Load(reader);

            var entity = new BasicUser();
            entity.Id = json._key;
            entity.IdentityId = json.identity_id;
            entity.Username = json.username;
            entity.Email = json.email;
            entity.IsEmailVerified = json.has_verified_email ?? false;
            entity.FirstName = json.first_name;
            entity.LastName = json.last_name;
            entity.MobileNumber = json.mobile_number;
            entity.IsMobileNumberVerified = json.has_verified_mobile_number ?? false;
            entity.Role = json.role ?? UserRoles.User;
            entity.IsBanned = json.is_banned ?? false;
            entity.ActiveDeviceId = json.active_device_id;

            return entity;
        }

        public override void WriteJson(JsonWriter writer, BasicUser? value, JsonSerializer serializer)
        {
            dynamic json = new JObject();
            json.identity_id = value.IdentityId;
            json.username = value.Username;
            json.email = value.Email;
            json.has_verified_email = value.IsEmailVerified;
            json.first_name = value.FirstName;
            json.last_name = value.LastName;
            json.mobile_number = value.MobileNumber;
            json.has_verified_mobile_number = value.IsMobileNumberVerified;
            json.role = value.Role.ToString();
            if (value.ActiveDeviceId != null)
            {
                json.active_device_id = value.ActiveDeviceId;
            }
            if (value.IsBanned)
            {
                json.is_banned = true;
            }
            json.WriteTo(writer);
        }
    }
}
