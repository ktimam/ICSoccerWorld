namespace Boom.Patterns.Broadcasts
{
    public interface IBroadcast
    {

    }
    public interface IBroadcastState
    {
        public abstract int MaxSavedStatesCount();
    }

    public enum DataState
    {
        None, Loading, Ready
    }

}