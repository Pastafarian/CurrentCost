namespace CurrentCost.Domain.Entities
{
	public class Message : Entity
	{
        public string Src { get; set; }
        public int Dsb { get; set; }
        public DateTime CreatedTime { get; set; }
        public double Tmpr { get; set; }
        public int Sensor { get; set; }
        public int Type { get; set; }
        public int Ch1Watts { get; set; }
        public int Ch2Watts { get; set; }
        public int Ch3Watts { get; set; }
        public int TotalWatts { get; set; }
    }
}
