using System.Collections.Generic;

namespace NuciCraft.API.Service.Generators
{
    public interface ISignIdGenerator
    {
        int GetNextId();

        List<string> GenerateDefaultSignIds(string nucicraftId);
    }
}
