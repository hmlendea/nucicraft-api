using NuciCraft.API.Requests;
using NuciCraft.API.Responses;

namespace NuciCraft.API.Service
{
    public interface IUserService
    {
        void AddUser(AddUserRequest request);
    }
}
