using Core.Api.Data;

namespace Core.Api.Service
{
    public interface IEntityManager<in TKey, in TEntity>
        where TKey : notnull
        where TEntity : class, IEntity<TKey>
    {
        bool Insert(TEntity entity);
        bool Update(TEntity entity);
        bool Delete(TKey key);
    }
}
