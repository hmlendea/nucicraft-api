using NuciAPI.Responses;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Responses
{
    public class GetResponse(object content) : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public object Content { get; set; } = content;
    }
}
