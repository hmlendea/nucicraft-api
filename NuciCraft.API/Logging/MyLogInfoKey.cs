using NuciLog.Core;

namespace NuciCraft.API.Logging
{
    public sealed class MyLogInfoKey : LogInfoKey
    {
        private MyLogInfoKey(string name) : base(name) { }

        public static LogInfoKey Biome => new MyLogInfoKey(nameof(Biome));
        public static LogInfoKey Count => new MyLogInfoKey(nameof(Count));
        public static LogInfoKey CreatedDT => new MyLogInfoKey(nameof(CreatedDT));
        public static LogInfoKey Identifier => new MyLogInfoKey(nameof(Identifier));
        public static LogInfoKey IpAddress => new MyLogInfoKey(nameof(IpAddress));
        public static LogInfoKey MobType => new MyLogInfoKey(nameof(MobType));
        public static LogInfoKey OfflineUUID => new MyLogInfoKey(nameof(OfflineUUID));
        public static LogInfoKey OnlineUUID => new MyLogInfoKey(nameof(OnlineUUID));
        public static LogInfoKey PlayerID => new MyLogInfoKey(nameof(PlayerID));
        public static LogInfoKey SkinUrl => new MyLogInfoKey(nameof(SkinUrl));
        public static LogInfoKey UpdatedDT => new MyLogInfoKey(nameof(UpdatedDT));
        public static LogInfoKey Username => new MyLogInfoKey(nameof(Username));
        public static LogInfoKey World => new MyLogInfoKey(nameof(World));
        public static LogInfoKey X => new MyLogInfoKey(nameof(X));
        public static LogInfoKey Y => new MyLogInfoKey(nameof(Y));
        public static LogInfoKey Z => new MyLogInfoKey(nameof(Z));
    }
}
