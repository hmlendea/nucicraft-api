using System;
using System.Collections.Generic;
using System.Linq;

namespace NuciCraft.API.Service.Models
{
    public sealed class MobType : IEquatable<MobType>
    {
        private static readonly Dictionary<string, MobType> values = new()
        {
            {
                nameof(Unsupported),
                new MobType(
                    nameof(Unsupported),
                    string.Empty)
            },
            {
                nameof(Cow),
                new MobType(
                    nameof(Cow),
                    "cow")
            },
            {
                nameof(EnderDragon),
                new MobType(
                    nameof(EnderDragon),
                    "ender_dragon")
            },
            {
                nameof(Evoker),
                new MobType(
                    nameof(Evoker),
                    "evoker")
            },
            {
                nameof(Illusioner),
                new MobType(
                    nameof(Illusioner),
                    "illusioner")
            },
            {
                nameof(Pillager),
                new MobType(
                    nameof(Pillager),
                    "pillager")
            },
            {
                nameof(Pig),
                new MobType(
                    nameof(Pig),
                    "pig")
            },
            {
                nameof(Villager),
                new MobType(
                    nameof(Villager),
                    "villager")
            },
            {
                nameof(Vindicator),
                new MobType(
                    nameof(Vindicator),
                    "vindicator")
            },
            {
                nameof(WanderingTrader),
                new MobType(
                    nameof(WanderingTrader),
                    "wandering_trader")
            }
        };

        public string Name { get; }

        public string ExternalName { get; }

        private MobType(
            string name,
            string externalName)
        {
            Name = name;
            ExternalName = externalName;
        }

        public static MobType Unsupported => values[nameof(Unsupported)];

        public static MobType Cow => values[nameof(Cow)];

        public static MobType EnderDragon => values[nameof(EnderDragon)];

        public static MobType Evoker => values[nameof(Evoker)];

        public static MobType Illusioner => values[nameof(Illusioner)];

        public static MobType Pillager => values[nameof(Pillager)];

        public static MobType Pig => values[nameof(Pig)];

        public static MobType WanderingTrader => values[nameof(WanderingTrader)];

        public static MobType Villager => values[nameof(Villager)];

        public static MobType Vindicator => values[nameof(Vindicator)];

        public static Array GetValues() => values.Values.ToArray();

        public static MobType FromString(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Unsupported;
            }

            MobType mobType = values.Values.FirstOrDefault(value =>
                string.Equals(value.ExternalName, name, StringComparison.OrdinalIgnoreCase));

            if (mobType is null)
            {
                return Unsupported;
            }

            return mobType;
        }

        public bool Equals(MobType other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                Name == other.Name &&
                ExternalName == other.ExternalName;
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

            return Equals((MobType)obj);
        }

        public override int GetHashCode() =>
            $"{nameof(MobType)}:{Name}:{ExternalName}".GetHashCode();

        public override string ToString() => ExternalName;

        public static bool operator ==(MobType current, MobType other)
        {
            if (current is null)
            {
                return other is null;
            }

            return current.Equals(other);
        }

        public static bool operator !=(MobType current, MobType other)
            => !(current == other);

        public static implicit operator string(MobType mobType) => mobType.ExternalName;
    }
}