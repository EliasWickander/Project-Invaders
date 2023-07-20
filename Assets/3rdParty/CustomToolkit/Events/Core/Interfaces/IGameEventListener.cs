namespace CustomToolkit.Events
{
    public interface IGameEventListener
    {
        void OnEventRaised();
    }

    public interface IGameEventListener<T>
    {
        void OnEventRaised(T value);
    }
    
    public interface INetworkGameEventListener<T>
    {
        void OnEventRaised(ConnectionType type, T value);
    }
}