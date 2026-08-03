using UnityEngine;

namespace CatMouse.Game.Meta
{
    public sealed class PlayerPrefsSaveStore : ISaveStore
    {
        public bool TryLoad(string key, out string payload)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                payload = string.Empty;
                return false;
            }

            payload = PlayerPrefs.GetString(key);
            return !string.IsNullOrEmpty(payload);
        }

        public void Save(string key, string payload)
        {
            PlayerPrefs.SetString(key, payload);
            PlayerPrefs.Save();
        }
    }
}
