using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrentCost.Monitor.Infrastructure.Deserialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CurrentCost.Monitor.Tests.Infrastructure.Deserialization
{
    public class MonitorMessageDeserializerTests
    {
        private Mock<ILogger<MonitorMessageDeserializer>> _loggerMock;

        private MonitorMessageDeserializer _monitorMessageDeserializer;
        public MonitorMessageDeserializerTests()
        {
            _loggerMock = new Mock<ILogger<MonitorMessageDeserializer>>();
            _monitorMessageDeserializer = new MonitorMessageDeserializer(_loggerMock.Object);
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
            Assert.Equal("CC128-v0.11", result.Src);
            Assert.Equal(00224, result.Dsb);
            Assert.Equal(02926, result.Id);
            Assert.Equal(0, result.Sensor);
            Assert.Equal(1, result.Type);
            result.Ch1.Watts.Should().BeInRange(01445, 5000);
            result.Ch2.Watts.Should().BeInRange(01445, 5000);
            result.Ch3.Watts.Should().BeInRange(01445, 5000);
            result.Time.Should().BeMoreThan(new TimeSpan(23, 20, 55))
                .Before(new DateTime(DateTime.Now.Year, 12, DateTime.Now.Day, 23, 22, 1));
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
