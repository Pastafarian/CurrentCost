namespace CurrentCost.Consumers.SignalR
{
    public record SignalRMonitorMessage
    {
        public required int Id { get; init; }
        public required string Src { get; init; }
        public required int Dsb { get; init; }
        public required DateTime CreatedTime { get; init; }
        public required double Tmpr { get; init; }
        public required int Sensor { get; init; }
        public required int Type { get; init; }
        public required int Ch1Watts { get; init; }
        public required int Ch2Watts { get; init; }
        public required int Ch3Watts { get; init; }
        public required int TotalWatts { get; init; }
    }
}
