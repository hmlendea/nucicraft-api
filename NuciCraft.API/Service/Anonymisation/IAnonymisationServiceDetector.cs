using System.Threading;
using System.Threading.Tasks;

namespace NuciCraft.API.Service.Anonymisation
{
    public interface IAnonymisationServiceDetector
    {
        Task<bool> IsAnonymisationService(string ipAddress, CancellationToken cancellationToken);
    }
}
