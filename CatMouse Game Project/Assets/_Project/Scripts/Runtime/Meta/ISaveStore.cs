namespace CatMouse.Game.Meta
{
    public interface ISaveStore
    {
        bool TryLoad(string key, out string payload);
        void Save(string key, string payload);
    }
}
