using System.Collections.Generic;

namespace NuciCraft.API.Service.Generators
{
    public interface ISignIdGenerator
    {
        List<string> GenerateDefaultSignIds(string nucicraftId);
    }
}
