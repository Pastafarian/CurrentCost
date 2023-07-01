using System.Xml.Serialization;

namespace CurrentCost.Messages.Messages
{
    public enum NotificationType
    {
        Email,
        Push,
        Sms
    }

    public interface INotificationCreated
    {
        DateTime NotificationDate { get; }
        string NotificationMessage { get; }
        NotificationType NotificationType { get; }
    }

    public interface IMessage
    {
        [XmlIgnore]
        public bool ShouldBeSent { get; }

        public int GetTotalWatts();
    }
}
