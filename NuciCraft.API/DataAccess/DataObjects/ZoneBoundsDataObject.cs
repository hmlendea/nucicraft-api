using NuciSecurity.HMAC;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public sealed class ZoneBoundsDataObject
    {
        [HmacOrder(1)]
        public CoordinatesDataObject FirstCorner { get; set; }

        [HmacOrder(2)]
        public CoordinatesDataObject SecondCorner { get; set; }
    }
}