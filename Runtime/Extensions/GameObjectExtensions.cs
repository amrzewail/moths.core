using UnityEngine;

namespace Moths.Extensions
{
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var c))
            {
                c = gameObject.gameObject.AddComponent<T>();
            }
            return c;
        }
    }
}