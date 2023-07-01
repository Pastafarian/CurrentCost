namespace CurrentCost.Messages.Messages
{
    public class UnknownMessage : IMessage
    {
        public bool ShouldBeSent => false;
        public int GetTotalWatts() => 0;
    }
}
