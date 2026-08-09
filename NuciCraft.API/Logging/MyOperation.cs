using NuciLog.Core;

namespace NuciCraft.API.Logging
{
    public sealed class MyOperation : Operation
    {
        private MyOperation(string name) : base(name) { }

        public static Operation AddRtpLocation => new MyOperation(nameof(AddRtpLocation));
        public static Operation GetAllZones => new MyOperation(nameof(GetAllZones));
        public static Operation GetRandomMobName => new MyOperation(nameof(GetRandomMobName));
        public static Operation GetPlayer => new MyOperation(nameof(GetPlayer));
        public static Operation GetRandomRtpLocation => new MyOperation(nameof(GetRandomRtpLocation));
        public static Operation GetZone => new MyOperation(nameof(GetZone));
        public static Operation PlayerDeath => new MyOperation(nameof(PlayerDeath));
        public static Operation RegisterPlayer => new MyOperation(nameof(RegisterPlayer));
        public static Operation UpdateLastDeathLocation => new MyOperation(nameof(UpdateLastDeathLocation));
        public static Operation UpdatePlayer => new MyOperation(nameof(UpdatePlayer));
        public static Operation UpdateZone => new MyOperation(nameof(UpdateZone));
    }
}
