using System;
using System.Collections.Generic;

using NuciDAL.Repositories;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.DataAccess.Repositories
{
    public sealed class OptimisticJsonRepository<TDataObject>(string storePath) : JsonRepository<TDataObject>(storePath)
        where TDataObject : NuciCraftEntityBase
    {
        public new void SaveChanges()
        {
            IEnumerable<TDataObject> allItems = [.. GetAll()];

            foreach (TDataObject item in allItems)
            {
                TDataObject existing = Get(item.Id);

                if (existing is not null && existing.Version != item.Version)
                {
                    throw new InvalidOperationException(
                        $"Optimistic concurrency conflict for entity '{item.Id}'. " +
                        $"Expected version {item.Version}, but found version {existing.Version}.");
                }
            }

            base.SaveChanges();
        }
    }
}