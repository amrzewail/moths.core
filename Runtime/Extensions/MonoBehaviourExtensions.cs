using UnityEngine;

namespace Moths.Extensions
{
    public static class MonoBehaviourExtensions
    {
        public static T GetOrAddComponent<T>(this MonoBehaviour behaviour) where T : Component => behaviour.gameObject.GetOrAddComponent<T>();
    }
}