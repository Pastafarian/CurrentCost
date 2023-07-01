using System.Xml.Serialization;

namespace CurrentCost.Messages.Messages
{
    
    [XmlRoot(ElementName = "ch1")]
    public class Ch1
    {
        [XmlElement(ElementName = "watts")] public int Watts { get; set; }
    }

    [XmlRoot(ElementName = "ch2")]
    public class Ch2
    {
        [XmlElement(ElementName = "watts")] public int Watts { get; set; }
    }

    [XmlRoot(ElementName = "ch3")]
    public class Ch3
    {
        [XmlElement(ElementName = "watts")] public int Watts { get; set; }
    }

    [XmlRoot(ElementName = "msg")]
    public class MonitorMessage : IMessage
    {
        [XmlIgnore]
        public bool ShouldBeSent => true;

        [XmlElement(ElementName = "src")] public string Src { get; set; }

        [XmlElement(ElementName = "dsb")] public int Dsb { get; set; }

        [XmlElement(ElementName = "time")] public string TimeString { get; set; }

        public DateTime Time => DateTime.Parse(TimeString);

        [XmlElement(ElementName = "tmpr")] public double Tmpr { get; set; }

        [XmlElement(ElementName = "sensor")] public int Sensor { get; set; }

        [XmlElement(ElementName = "id")] public int Id { get; set; }

        [XmlElement(ElementName = "type")] public int Type { get; set; }

        [XmlElement(ElementName = "ch1")] public Ch1 Ch1 { get; set; }

        [XmlElement(ElementName = "ch2")] public Ch2 Ch2 { get; set; }

        [XmlElement(ElementName = "ch3")] public Ch3 Ch3 { get; set; }

        public int GetTotalWatts() => Ch1.Watts + Ch2.Watts + Ch3.Watts;
    }
}
