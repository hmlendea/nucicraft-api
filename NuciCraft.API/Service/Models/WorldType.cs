using System;
using System.Collections.Generic;
using System.Linq;

namespace NuciCraft.API.Service.Models
{
    public sealed class WorldType : IEquatable<WorldType>
    {
        private static readonly Dictionary<string, WorldType> values = new()
        {
            { nameof(Overworld), new WorldType(nameof(Overworld), "overworld") },
            { nameof(Nether), new WorldType(nameof(Nether), "nether") },
            { nameof(End), new WorldType(nameof(End), "end") }
        };

        public string Name { get; }

        public string ExternalName { get; }

        private WorldType(string name, string externalName)
        {
            Name = name;
            ExternalName = externalName;
        }

        public static WorldType Overworld => values[nameof(Overworld)];

        public static WorldType Nether => values[nameof(Nether)];

        public static WorldType End => values[nameof(End)];

        public static Array GetValues() => values.Values.ToArray();

        public static WorldType FromString(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Overworld;
            }

            WorldType worldType = values.Values.FirstOrDefault(value =>
                string.Equals(value.ExternalName, name, StringComparison.OrdinalIgnoreCase));

            if (worldType is null)
            {
                return Overworld;
            }

            return worldType;
        }

        public bool Equals(WorldType other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Name == other.Name && ExternalName == other.ExternalName;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((WorldType)obj);
        }

        public override int GetHashCode() => $"{nameof(WorldType)}:{Name}:{ExternalName}".GetHashCode();

        public override string ToString() => ExternalName;

        public static bool operator ==(WorldType current, WorldType other)
        {
            if (current is null)
            {
                return other is null;
            }

            return current.Equals(other);
        }

        public static bool operator !=(WorldType current, WorldType other) => !(current == other);

        public static implicit operator string(WorldType worldType) => worldType.ExternalName;
    }
}