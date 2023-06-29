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

    public abstract class BaseMessage
    {
        [XmlIgnore]
        public abstract bool ShouldBeSent { get; }

        public abstract int GetTotalWatts();
    }
}
