using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using NuciDAL.DataObjects;
using NuciDAL.Repositories;
using NuciExtensions;

namespace NuciCraft.API.IntegrationTests
{
    internal sealed class InMemoryRepository<TDataObject> : IFileRepository<TDataObject>
        where TDataObject : EntityBase
    {
        private readonly ConcurrentDictionary<string, TDataObject> entities = new();
        private readonly object syncRoot = new();

        public int EntitiesCount => entities.Count;

        public void Add(TDataObject entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            TDataObject clone = CloneEntity(entity);
            if (!entities.TryAdd(clone.Id, clone))
            {
                throw new EntityAlreadyExistsException(clone.Id, typeof(TDataObject));
            }
        }

        public bool TryAdd(TDataObject entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            TDataObject clone = CloneEntity(entity);
            return entities.TryAdd(clone.Id, clone);
        }

        public bool ContainsId(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            return entities.ContainsKey(id);
        }

        public TDataObject Get(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            if (!entities.TryGetValue(id, out TDataObject value))
            {
                throw new EntityNotFoundException(id, typeof(TDataObject));
            }
            return CloneEntity(value);
        }

        public TDataObject GetRandom()
        {
            lock (syncRoot)
            {
                if (entities.IsEmpty)
                {
                    throw new EntityNotFoundException("No entities available", typeof(TDataObject));
                }
                return CloneEntity(entities.Values.GetRandomElement());
            }
        }

        public TDataObject GetFirst(Func<TDataObject, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            TDataObject result = TryGetFirst(predicate);
            if (result is null)
            {
                throw new EntityNotFoundException(typeof(TDataObject));
            }
            return result;
        }

        public TDataObject TryGet(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            if (!entities.TryGetValue(id, out TDataObject value))
            {
                return null;
            }
            return CloneEntity(value);
        }

        public TDataObject TryGetFirst(Func<TDataObject, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return CloneEntity(GetAll().FirstOrDefault(predicate));
        }

        public IEnumerable<TDataObject> GetAll()
        {
            lock (syncRoot)
            {
                return entities.Values.Select(CloneEntity).ToList().AsReadOnly();
            }
        }

        public IEnumerable<TDataObject> Find(Func<TDataObject, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return GetAll().Where(predicate).ToList().AsReadOnly();
        }

        public void Update(TDataObject entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            TDataObject clone = CloneEntity(entity);
            lock (syncRoot)
            {
                if (!entities.TryGetValue(clone.Id, out _))
                {
                    throw new EntityNotFoundException(clone.Id, typeof(TDataObject));
                }
                entities[clone.Id] = clone;
            }
        }

        public bool TryUpdate(TDataObject entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            TDataObject clone = CloneEntity(entity);
            lock (syncRoot)
            {
                if (!entities.TryGetValue(clone.Id, out TDataObject existing))
                {
                    return false;
                }
                return entities.TryUpdate(clone.Id, clone, existing);
            }
        }

        public void Upsert(TDataObject entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            TDataObject clone = CloneEntity(entity);
            lock (syncRoot)
            {
                entities[clone.Id] = clone;
            }
        }

        public void Remove(TDataObject entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            lock (syncRoot)
            {
                if (!entities.TryRemove(entity.Id, out _))
                {
                    throw new EntityNotFoundException(entity.Id, typeof(TDataObject));
                }
            }
        }

        public void Remove(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            lock (syncRoot)
            {
                if (!entities.TryRemove(id, out _))
                {
                    throw new EntityNotFoundException(id, typeof(TDataObject));
                }
            }
        }

        public bool TryRemove(TDataObject entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            lock (syncRoot)
            {
                return entities.TryRemove(entity.Id, out _);
            }
        }

        public bool TryRemove(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            lock (syncRoot)
            {
                return entities.TryRemove(id, out _);
            }
        }

        public void SaveChanges()
        {
        }

        private static TDataObject CloneEntity(TDataObject entity)
        {
            if (entity is null)
            {
                return null;
            }
            string json = JsonSerializer.Serialize(entity);
            return JsonSerializer.Deserialize<TDataObject>(json);
        }
    }
}