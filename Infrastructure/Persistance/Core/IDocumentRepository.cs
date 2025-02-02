namespace Infrastructure.Persistance.Core
{
    internal interface IDocumentRepository<T> where T : class
    {
        Task<T> Add(T entity, string collectionName);
        Task<List<T>> AddMany(List<T> entity, string collectionName);
        Task<T> Get(string key, string collectionName);
        Task<IEnumerable<T>> Execute(string query, Dictionary<string, object>? bindVars = null);
        Task<T> Update(string key, string collectionName, T entity);
        Task Delete(string key, string collectionName);
    }
}
