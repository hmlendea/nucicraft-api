using NuciLog.Core;

namespace NuciCraft.API.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name) : base(name) { }

        public static Operation AddRtpLocation => new MyOperation(nameof(AddRtpLocation));
        public static Operation GetRandomRtpLocation => new MyOperation(nameof(GetRandomRtpLocation));
        public static Operation GetUser => new MyOperation(nameof(GetUser));
        public static Operation RegisterUser => new MyOperation(nameof(RegisterUser));
    }
}
