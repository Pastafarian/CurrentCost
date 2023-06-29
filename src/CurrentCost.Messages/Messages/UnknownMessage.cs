namespace CurrentCost.Messages.Messages
{
    public class UnknownMessage : BaseMessage
    {
        public override bool ShouldBeSent => false;
        public override int GetTotalWatts() => 0;
    }
}
