using NuciCraft.API.Requests;

namespace NuciCraft.API.Service
{
    public interface IUserService
    {
        void Register(RegisterUserRequest request);
    }
}
