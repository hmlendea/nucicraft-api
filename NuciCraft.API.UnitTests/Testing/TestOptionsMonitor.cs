using System;
using Microsoft.Extensions.Options;

namespace NuciCraft.API.UnitTests.Testing
{
    internal sealed class TestOptionsMonitor<TOptions>(TOptions value) : IOptionsMonitor<TOptions>
        where TOptions : class, new()
    {
        public TOptions CurrentValue => value;

        public TOptions Get(string name) => value;

        public IDisposable OnChange(Action<TOptions, string?> listener) => new NoopDisposable();
    }
}