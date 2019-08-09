namespace UniFlow
{
    public abstract class EventReceiver : EventConnectable, IEventReceiver
    {
        public abstract void OnReceive(EventMessages eventMessages);
    }
}