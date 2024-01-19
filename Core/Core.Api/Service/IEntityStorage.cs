using Core.Api.Data;

namespace Core.Api.Service
{
    public interface IEntityStorage<in TKey, TEntity> : IEntityManager<TKey, TEntity>, IEntityProvider<TKey, TEntity>
        where TKey : notnull
        where TEntity : class, IEntity<TKey>
    {
    }
}
