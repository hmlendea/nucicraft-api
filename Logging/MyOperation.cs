using NuciLog.Core;

namespace NuciCraft.API.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name) : base(name) { }

        public static Operation AddRtpLocation => new MyOperation(nameof(AddRtpLocation));
        public static Operation AddUser => new MyOperation(nameof(AddUser));
        public static Operation GetRandomRtpLocation => new MyOperation(nameof(GetRandomRtpLocation));
    }
}
