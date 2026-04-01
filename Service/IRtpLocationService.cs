using NuciCraft.API.Requests;
using NuciCraft.API.Responses;

namespace NuciCraft.API.Service
{
    public interface IRtpLocationService
    {
        void AddRtpLocation(AddRtpLocationRequest request);

        GetRtpLocationResponse GetRtpLocation(GetRtpLocationRequest request);
    }
}
