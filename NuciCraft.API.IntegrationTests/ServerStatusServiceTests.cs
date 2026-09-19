using System;
using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NuciCraft.API.Configuration;
using NuciCraft.API.Service;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    public sealed class ServerStatusServiceTests
    {
        private static TimeSpan TestTimeout => TimeSpan.FromSeconds(10);

        private TcpListener listener = null!;
        private ServerStatusService service = null!;

        [SetUp]
        public void SetUp()
        {
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            IPEndPoint endpoint = (IPEndPoint)listener.LocalEndpoint;
            service = new ServerStatusService(new ServerSettings
            {
                Hostname = endpoint.Address.ToString(),
                JavaEditionPort = endpoint.Port,
                BedrockEditionPort = 873
            });
        }

        [TearDown]
        public void TearDown() => listener.Stop();

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(42)]
        [TestCase(613)]
        [TestCase(int.MaxValue)]
        public async Task GivenAJavaStatusResponse_WhenQueryingTheServer_ThenItsLiveCountIsReturned(
            int onlinePlayersCount)
        {
            int result = await QueryServerAsync(
                BuildStatusPayload(onlinePlayersCount.ToString(CultureInfo.InvariantCulture)));

            Assert.That(result, Is.EqualTo(onlinePlayersCount));
        }

        [Test]
        public async Task GivenChangingPlayerCounts_WhenQueryingTwice_ThenEachCountIsRetrievedLive()
        {
            int firstCount = await QueryServerAsync(BuildStatusPayload("42"));
            int secondCount = await QueryServerAsync(BuildStatusPayload("0"));

            Assert.That(firstCount, Is.EqualTo(42));
            Assert.That(secondCount, Is.Zero);
        }

        [TestCase("DummyUser")]
        public async Task GivenAnUnavailableStatus_WhenQueryingTheServer_ThenZeroIsReturned(string payload)
        {
            int result = await QueryServerAsync(payload);

            Assert.That(result, Is.Zero);
        }

        [Test]
        public void GivenANegativePlayerCount_WhenQueryingTheServer_ThenTheQueryFails()
            => Assert.That(
                async () => await QueryServerAsync(BuildStatusPayload("-1")),
                Throws.InvalidOperationException);

        [TestCase("")]
        [TestCase("DummyUser")]
        public void GivenANonNumericPlayerCount_WhenQueryingTheServer_ThenTheQueryFails(string count)
            => Assert.That(
                async () => await QueryServerAsync(BuildStatusPayload(count)),
                Throws.TypeOf<FormatException>());

        [Test]
        public void GivenAnUnavailableServer_WhenQueryingItsPlayerCount_ThenZeroIsReturned()
        {
            listener.Stop();

            Assert.That(service.GetOnlinePlayersCount(), Is.Zero);
        }

        [Test]
        public async Task GivenAResponseTimeout_WhenQueryingItsPlayerCount_ThenZeroIsReturned()
        {
            int result = await QueryServerAsync(string.Empty);

            Assert.That(result, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task GivenNoServerReply_WhenQueryingItsPlayerCount_ThenZeroIsReturned(
            bool connectionIsClosed)
        {
            Task<int> query = Task.Run(service.GetOnlinePlayersCount);
            using TcpClient connection = await listener.AcceptTcpClientAsync().WaitAsync(TestTimeout);

            if (connectionIsClosed)
            {
                connection.Close();
            }

            int result = await query.WaitAsync(TestTimeout);

            Assert.That(result, Is.Zero);
        }

        private async Task<int> QueryServerAsync(string payload)
        {
            Task<int> query = Task.Run(service.GetOnlinePlayersCount);
            using TcpClient connection = await listener.AcceptTcpClientAsync().WaitAsync(TestTimeout);
            using NetworkStream stream = connection.GetStream();
            byte[] request = new byte[2];
            await stream.ReadExactlyAsync(request).AsTask().WaitAsync(TestTimeout);

            Assert.That(request, Is.EqualTo(new byte[] { 0xfe, 0x01 }));

            byte[] response = BuildStatusPacket(payload);
            await stream.WriteAsync(response).AsTask().WaitAsync(TestTimeout);

            return await query.WaitAsync(TestTimeout);
        }

        private static string BuildStatusPayload(string count)
            => string.Join(
                '\0',
                "\u00a71",
                "127",
                "1.21",
                "Testy McTestface",
                count,
                int.MaxValue.ToString(CultureInfo.InvariantCulture));

        private static byte[] BuildStatusPacket(string payload)
        {
            byte[] encodedPayload = Encoding.BigEndianUnicode.GetBytes(payload);
            byte[] packet = new byte[3 + encodedPayload.Length];
            packet[0] = 0xff;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(1), (ushort)payload.Length);
            encodedPayload.CopyTo(packet, 3);

            return packet;
        }
    }
}