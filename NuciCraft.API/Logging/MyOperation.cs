using NuciLog.Core;

namespace NuciCraft.API.Logging
{
    public sealed class MyOperation : Operation
    {
        private MyOperation(string name) : base(name) { }

        public static Operation AddCountry => new MyOperation(nameof(AddCountry));
        public static Operation AddRtpLocation => new MyOperation(nameof(AddRtpLocation));
        public static Operation AddWorld => new MyOperation(nameof(AddWorld));
        public static Operation AddZone => new MyOperation(nameof(AddZone));
        public static Operation DeleteZone => new MyOperation(nameof(DeleteZone));
        public static Operation GetAllCountries => new MyOperation(nameof(GetAllCountries));
        public static Operation GetAllPlayers => new MyOperation(nameof(GetAllPlayers));
        public static Operation GetAllWorlds => new MyOperation(nameof(GetAllWorlds));
        public static Operation GetAllZones => new MyOperation(nameof(GetAllZones));
        public static Operation GetZonesByCoordinates => new MyOperation(nameof(GetZonesByCoordinates));
        public static Operation GetCountry => new MyOperation(nameof(GetCountry));
        public static Operation GetPlayer => new MyOperation(nameof(GetPlayer));
        public static Operation GetRandomMobName => new MyOperation(nameof(GetRandomMobName));
        public static Operation GetRandomRtpLocation => new MyOperation(nameof(GetRandomRtpLocation));
        public static Operation GetWorld => new MyOperation(nameof(GetWorld));
        public static Operation GetZone => new MyOperation(nameof(GetZone));
        public static Operation RegisterPlayer => new MyOperation(nameof(RegisterPlayer));
        public static Operation UpdateCountry => new MyOperation(nameof(UpdateCountry));
        public static Operation UpdatePlayer => new MyOperation(nameof(UpdatePlayer));
        public static Operation UpdateWorld => new MyOperation(nameof(UpdateWorld));
        public static Operation UpdateZone => new MyOperation(nameof(UpdateZone));
    }
}
