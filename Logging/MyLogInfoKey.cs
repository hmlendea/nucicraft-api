using NuciLog.Core;

namespace NuciCraft.API.Logging
{
    public sealed class MyLogInfoKey : LogInfoKey
    {
        MyLogInfoKey(string name) : base(name) { }

        public static LogInfoKey Biome => new MyLogInfoKey(nameof(Biome));
        public static LogInfoKey OfflineUUID => new MyLogInfoKey(nameof(OfflineUUID));
        public static LogInfoKey OnlineUUID => new MyLogInfoKey(nameof(OnlineUUID));
        public static LogInfoKey SkinUrl => new MyLogInfoKey(nameof(SkinUrl));
        public static LogInfoKey Username => new MyLogInfoKey(nameof(Username));
        public static LogInfoKey World => new MyLogInfoKey(nameof(World));
        public static LogInfoKey X => new MyLogInfoKey(nameof(X));
        public static LogInfoKey Y => new MyLogInfoKey(nameof(Y));
        public static LogInfoKey Z => new MyLogInfoKey(nameof(Z));
    }
}
