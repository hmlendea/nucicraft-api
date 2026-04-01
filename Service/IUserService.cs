using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IUserService
    {
        void Register(RegisterUserRequest request);

        User Get(string username);
    }
}
