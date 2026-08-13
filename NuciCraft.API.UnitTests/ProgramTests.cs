using System;
using System.IO;

using Microsoft.Extensions.Hosting;

using NUnit.Framework;

namespace NuciCraft.API.UnitTests
{
    [TestFixture]
    public sealed class ProgramTests
    {
        [Test]
        public void GivenConfigurationArguments_WhenCreatingAHostBuilder_ThenTheHostCanBeBuilt()
        {
            string storeDirectory = Path.Combine(
                Path.GetTempPath(),
                nameof(ProgramTests),
                Guid.NewGuid().ToString());
            string[] arguments = TestConfigurationFactory.BuildCommandLineArguments(storeDirectory);

            using IHost host = Program.CreateHostBuilder(arguments).Build();

            Assert.That(host, Is.Not.Null);
        }

        [Test]
        public void GivenAnInvalidStorePath_WhenRunningMain_ThenStartupFailsBeforeTheServerLoop()
            => Assert.That(
                () => Program.Main([
                    "--dataStoreSettings:rtpLocationsStorePath",
                    " ",
                    "--nuciLoggerSettings:isFileOutputEnabled",
                    "false",
                ]),
                Throws.Exception);
    }
}