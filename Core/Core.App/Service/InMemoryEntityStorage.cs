using Core.Api.Data;
using Core.Api.Service;
using System.Collections.Concurrent;

namespace Core.App.Service
{
    public class InMemoryEntityStorage<TKey, TEntity> : IEntityStorage<TKey, TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, TEntity> storage = new();

        public InMemoryEntityStorage()
        {
            storage = new();
        }

        public InMemoryEntityStorage(ConcurrentDictionary<TKey, TEntity> storage)
        {
            this.storage = storage;
        }

        public bool Insert(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return this.storage.TryAdd(entity.Key, entity);
        }

        public bool Update(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!this.storage.ContainsKey(entity.Key))
            {
                throw new KeyNotFoundException();
            }

            this.storage[entity.Key] = entity;

            return true;
        }

        public bool Delete(TKey key)
        {
            return this.storage.TryRemove(key, out _);
        }

        public TEntity? GetEntity(TKey key)
        {
            this.storage.TryGetValue(key, out TEntity? value);

            return value;
        }

        public IEnumerable<TEntity> GetAll()
        {
            return this.storage.Values;
        }
    }
}
