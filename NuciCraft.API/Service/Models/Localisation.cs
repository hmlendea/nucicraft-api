using System;
using System.Collections.Generic;
using System.Linq;

namespace NuciCraft.API.Service.Models
{
    public sealed class Localisation : IEquatable<Localisation>
    {
        private static readonly Dictionary<string, Localisation> values = new()
        {
            {
                nameof(Unsupported),
                new Localisation(
                    nameof(Unsupported),
                    string.Empty)
            },
            {
                nameof(English),
                new Localisation(
                    nameof(English),
                    "english")
            },
            {
                nameof(Romanian),
                new Localisation(
                    nameof(Romanian),
                    "romanian")
            }
        };

        public string Name { get; }

        public string ExternalName { get; }

        private Localisation(
            string name,
            string externalName)
        {
            Name = name;
            ExternalName = externalName;
        }

        public static Localisation Unsupported => values[nameof(Unsupported)];

        public static Localisation English => values[nameof(English)];

        public static Localisation Romanian => values[nameof(Romanian)];

        public static Array GetValues() => values.Values.ToArray();

        public static Localisation FromString(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Unsupported;
            }

            Localisation localisation = values.Values.FirstOrDefault(value =>
                string.Equals(value.ExternalName, name, StringComparison.OrdinalIgnoreCase));

            if (localisation is null)
            {
                return Unsupported;
            }

            return localisation;
        }

        public bool Equals(Localisation other)
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

            return Equals((Localisation)obj);
        }

        public override int GetHashCode() =>
            $"{nameof(Localisation)}:{Name}:{ExternalName}".GetHashCode();

        public override string ToString() => ExternalName;

        public static bool operator ==(Localisation current, Localisation other)
        {
            if (current is null)
            {
                return other is null;
            }

            return current.Equals(other);
        }

        public static bool operator !=(Localisation current, Localisation other)
            => !(current == other);

        public static implicit operator string(Localisation localisation) => localisation.ExternalName;
    }
}
