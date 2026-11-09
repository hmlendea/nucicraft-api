using System;

namespace NuciCraft.API.UnitTests.Testing
{
    internal sealed class NoopDisposable : IDisposable
    {
        public void Dispose() { }
    }
}