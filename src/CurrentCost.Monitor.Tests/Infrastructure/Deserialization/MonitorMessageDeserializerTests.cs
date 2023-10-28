using CurrentCost.Monitor.Infrastructure.Deserialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CurrentCost.Monitor.Tests.Infrastructure.Deserialization
{
    public class MonitorMessageDeserializerTests
    {
        private readonly MonitorMessageDeserializer _monitorMessageDeserializer;
        public MonitorMessageDeserializerTests()
        {
            Mock<ILogger<MonitorMessageDeserializer>> loggerMock = new();
            _monitorMessageDeserializer = new MonitorMessageDeserializer(loggerMock.Object);
        }

        [Theory]
        [InlineData("<msg><src>CC128-v0.11</src><dsb>00224</dsb><time>23:20:56</time><tmpr>21.5</tmpr><sensor>0</sensor><id>02926</id><type>1</type><ch1><watts>01445</watts></ch1><ch2><watts>01957</watts></ch2><ch3><watts>04110</watts></ch3></msg>")]
        [InlineData("<msg><src>CC128-v0.11</src><dsb>00224</dsb><time>23:21:02</time><tmpr>21.6</tmpr><sensor>0</sensor><id>02926</id><type>1</type><ch1><watts>01458</watts></ch1><ch2><watts>01977</watts></ch2><ch3><watts>04112</watts></ch3></msg>")]
        [InlineData("<msg><src>CC128-v0.11</src><dsb>00224</dsb><time>23:21:09</time><tmpr>21.6</tmpr><sensor>0</sensor><id>02926</id><type>1</type><ch1><watts>01471</watts></ch1><ch2><watts>03264</watts></ch2><ch3><watts>04081</watts></ch3></msg>")]
        [InlineData("<msg><src>CC128-v0.11</src><dsb>00224</dsb><time>23:21:15</time><tmpr>21.6</tmpr><sensor>0</sensor><id>02926</id><type>1</type><ch1><watts>01462</watts></ch1><ch2><watts>01977</watts></ch2><ch3><watts>04116</watts></ch3></msg>")]
        [InlineData("<msg><src>CC128-v0.11</src><dsb>00224</dsb><time>23:21:21</time><tmpr>21.6</tmpr><sensor>0</sensor><id>02926</id><type>1</type><ch1><watts>01449</watts></ch1><ch2><watts>01998</watts></ch2><ch3><watts>04000</watts></ch3></msg>")]
        public void Deserialize_ValidMessage_ReturnsMonitorMessage(string message)
        {
            var result = _monitorMessageDeserializer.Deserialize(message);

            result.Should().NotBeNull();
            result.Src.Should().Be("CC128-v0.11");
            result.Dsb.Should().Be(00224);
            result.Id.Should().Be(02926);
            result.Sensor.Should().Be(0);
            result.Type.Should().Be(1);
            result.Ch1.Watts.Should().BeInRange(01445, 5000);
            result.Ch2.Watts.Should().BeInRange(01445, 5000);
            result.Ch3.Watts.Should().BeInRange(01445, 5000);
            result.Tmpr.Should().BeInRange(21.5, 21.6);
        }

        [Theory]
        [InlineData("<msg><src>CC128-v0.11</src><dsb>00224</dsb><time>0:41:19</time><tmpr>21.5</tmpr><sensor>0</sensor><id>02926</id><type>1</type><ch1><watts>1464</watts></ch1><ch2><watts>1986</watts></ch2><ch3><watts>4081</watts></ch3></msg>")]
        public void Deserialize_InvalidMessage_ReturnsNull(string message)
        {
            var result = _monitorMessageDeserializer.Deserialize(message);
            result.Should().NotBeNull();
        }
    }
}
