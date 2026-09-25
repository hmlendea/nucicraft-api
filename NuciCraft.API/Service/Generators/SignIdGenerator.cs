using System;
using System.Collections.Generic;
using System.Linq;

namespace NuciCraft.API.Service.Generators
{
    public sealed class SignIdGenerator : ISignIdGenerator
    {
        public List<string> GenerateDefaultSignIds(string minecraftId)
        {
            string[] parts = minecraftId.Split(':', 2);
            string afterColon = parts.Length > 1 ? parts[1] : string.Empty;
            string[] words = afterColon
                .Replace('_', ' ')
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (words.Length == 0)
            {
                return [];
            }

            string signId = string.Concat(
                words.Select(word =>
                    char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));

            return [signId];
        }
    }
}
