using System;
using System.Collections.Generic;
using System.Linq;

namespace NuciCraft.API.Service.Models
{
    public sealed class Gender : IEquatable<Gender>
    {
        private static readonly Dictionary<string, Gender> values = new()
        {
            { nameof(Male), new Gender(nameof(Male), "male") },
            { nameof(Female), new Gender(nameof(Female), "female") },
            { nameof(Other), new Gender(nameof(Other), "other") }
        };

        public string Name { get; }

        public string ExternalName { get; }

        private Gender(string name, string externalName)
        {
            Name = name;
            ExternalName = externalName;
        }

        public static Gender Male => values[nameof(Male)];

        public static Gender Female => values[nameof(Female)];

        public static Gender Other => values[nameof(Other)];

        public static Array GetValues() => values.Values.ToArray();

        public static Gender FromString(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Other;
            }

            Gender gender = values.Values.FirstOrDefault(value =>
                string.Equals(value.ExternalName, name, StringComparison.OrdinalIgnoreCase));

            if (gender is null)
            {
                return Other;
            }

            return gender;
        }

        public bool Equals(Gender other)
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

            return Equals((Gender)obj);
        }

        public override int GetHashCode() => $"{nameof(Gender)}:{Name}:{ExternalName}".GetHashCode();

        public override string ToString() => ExternalName;

        public static bool operator ==(Gender current, Gender other)
        {
            if (current is null)
            {
                return other is null;
            }

            return current.Equals(other);
        }

        public static bool operator !=(Gender current, Gender other) => !(current == other);

        public static implicit operator string(Gender gender) => gender.ExternalName;
    }
}