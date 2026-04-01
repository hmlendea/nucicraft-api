using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IRtpLocationService
    {
        void AddRtpLocation(AddRtpLocationRequest request);

        RtpLocation GetRtpLocation(GetRtpLocationRequest request);
    }
}
