using System;
using System.Collections.Generic;
using System.Linq;

namespace NuciCraft.API.Service.Generators
{
    public sealed class SignIdGenerator : ISignIdGenerator
    {
        public List<string> GenerateDefaultSignIds(string nucicraftId)
        {
            string signId = nucicraftId;

            if (signId.Contains(':'))
            {
                signId = signId[(signId.IndexOf(':') + 1)..];
            }

            signId = string.Concat(signId
                .Replace('_', ' ')
                .Split(' ')
                .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant())
                .ToArray());

            if (string.IsNullOrWhiteSpace(signId))
            {
                throw new ArgumentException(
                    "Invalid NuciCraft ID.",
                    nameof(nucicraftId));
            }

            signId = ApplyCustomReplacements(signId);
            signId = TrimTo16Characters(signId);

            return [signId];
        }

        private static string ApplyCustomReplacements(string input)
        {
            Dictionary<string, string> customReplacements = new()
            {
                { "Axe", "Hatchet" },
                { "Cherry", "Sakura" },
                { "Cobblestone", "Cobble" },
                { "Concrete", "Cement" },
                { "Deepslate", "Deep" },
                { "Leaves", "Foliage" },
                { "Stained", string.Empty },
                { "Sulfur", "Sulphur" },
            };

            foreach (var replacement in customReplacements)
            {
                input = input.Replace(replacement.Key, replacement.Value);
            }

            return input;
        }

        private static string TrimTo16Characters(string input)
        {
            if (input.Length <= 16)
            {
                return input;
            }

            Dictionary<string, string> wordShortenings = new()
            {
                { "Netherite", "Nether" },

                { "Chiselled", "Chisel" },
                { "Polished", "Polish" },

                { "Cinnabar", "Cinbar" },
                { "Nether", "Nthr" },
                { "Pearlescent", "Pearl" },
                { "Smithing", "Smith" },

                { "Foliage", "Leaves" },
                { "Hatchet", "Axe" },

                { "Pickaxe", "Pick" },
                { "Leaves", "Leaf" },
                { "Stairs", "Stair" },

                { "Polish", "Poli" },
                { "Tuff", "Tuf" },
                { "Upgrade", "Upg" },
            };

            foreach (var wordShortening in wordShortenings)
            {
                if (input.Length <= 16)
                {
                    break;
                }

                input = input.Replace(wordShortening.Key, wordShortening.Value);
            }

            if (input.Length > 16)
            {
                return input[..16];
            }

            return input;
        }
    }
}
