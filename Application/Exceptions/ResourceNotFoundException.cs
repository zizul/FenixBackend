
namespace Application.Exceptions
{
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(string message)
            : base(message)
        {
        }

        public ResourceNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public static ResourceNotFoundException WithId<T>(string id)
        {
            var typeStr = typeof(T).Name;
            var msg = $"Resource {typeStr} with id {id} not found";
            return new ResourceNotFoundException(msg);
        }
    }
}
