using UnityEngine;

namespace Moths.Extensions
{
    public static class TransformExtensions
    {
        public static T FindComponent<T>(this Transform transform, string path)
        {
            return transform.Find(path).GetComponent<T>();
        }

        public static T FindComponentInParents<T>(this Transform transform)
        {
            Transform t = transform;
            T c = default(T);

            while (!t.TryGetComponent<T>(out c) && t.parent)
            {
                t = t.parent;
            }
            return c;
        }

        public static string GetPath(this Transform transform, Transform from = null)
        {
            string path = from == null ? "/" + transform.name : "";
            while (transform != from)
            {
                path = "/" + transform.name + path;
                transform = transform.parent;
            }

            if (path.Length > 0) path = path.TrimStart('/');

            return path;
        }

    }
}