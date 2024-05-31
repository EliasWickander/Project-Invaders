namespace CustomToolkit.Mirror.Events
{
    public interface INetworkGameEventListener<T>
    {
        void OnEventRaised(ConnectionType type, T value);
    }
}