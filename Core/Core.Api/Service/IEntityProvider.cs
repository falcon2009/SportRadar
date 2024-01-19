using Core.Api.Data;

namespace Core.Api.Service
{
    public interface IEntityProvider<in TKey, out TEntity>
        where TKey : notnull
        where TEntity : class, IEntity<TKey>
    {
        TEntity? GetEntity(TKey key);
        IEnumerable<TEntity> GetAll();
    }
}
