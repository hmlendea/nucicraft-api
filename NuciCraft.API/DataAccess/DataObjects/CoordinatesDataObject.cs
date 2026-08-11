using NuciSecurity.HMAC;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public sealed class CoordinatesDataObject
    {
        [HmacOrder(1)]
        public string World { get; set; }

        [HmacOrder(2)]
        public float X { get; set; }

        [HmacOrder(3)]
        public float Y { get; set; }

        [HmacOrder(4)]
        public float Z { get; set; }

        [HmacOrder(5)]
        public float Pitch { get; set; } = 0.0f;

        [HmacOrder(6)]
        public float Yaw { get; set; } = 179.9f;
    }
}
