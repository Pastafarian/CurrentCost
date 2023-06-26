using System.Diagnostics.Metrics;

namespace CurrentCost.Monitor.Infrastructure
{
    public class CurrentCostMonitorMetrics
    {
        private Counter<int> MessagesReceivedCounter { get; }
        private Counter<int> TotalWattage { get; }

        public string MetricName { get; }

        public CurrentCostMonitorMetrics(string meterName = "CurrentCostMonitorMetrics")
        {
            var meter = new Meter(meterName);
            MetricName = meterName;

            MessagesReceivedCounter = meter.CreateCounter<int>("messages-received", "Messages");
            TotalWattage = meter.CreateCounter<int>("total-wattage", "Messages");
        }
        public void MessagesReceive() => MessagesReceivedCounter.Add(1);
        public void RecordTotalWattage(int wattage) => TotalWattage.Add(wattage);
        
    }
}
