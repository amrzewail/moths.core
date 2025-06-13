using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Collections
{
    [System.Serializable]
    public struct Tag
    {
        private static Dictionary<string, GameObject> _gameObjects;

        [SerializeField] private string tag;

        public string Value { get { return tag; } }

        public Tag(string tag) => this.tag = tag;

        public static implicit operator Tag(string tag) => new Tag(tag);

        public static implicit operator string(Tag tag) => tag.tag;

        public GameObject GameObject => GetGameObject(Value);
        public Transform Transform => GameObject.transform;


        private static Dictionary<(string, Type), Component> _getComponent;
        private static Dictionary<(string, Type), Component> _getComponentInChildren;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _getComponent = new Dictionary<(string, Type), Component>(128);
            _getComponentInChildren = new Dictionary<(string, Type), Component>(128);
        }

        public static GameObject GetGameObject(string tag)
        {
            if (_gameObjects.ContainsKey(tag) && _gameObjects[tag])
            {
                return _gameObjects[tag];
            }
            return _gameObjects[tag] = GameObject.FindGameObjectWithTag(tag);
        }


        public T GetComponent<T>() where T : Component
        {
            var key = (Value, typeof(T));
            if (!_getComponent.TryGetValue(key, out Component component) || !component)
            {
                component = GameObject.GetComponent<T>();
                if (component) _getComponent[key] = component;
            }
            return (T)component;
        }

        public T GetComponentInChildren<T>() where T : Component
        {
            var key = (Value, typeof(T));
            if (!_getComponentInChildren.TryGetValue(key, out Component component) || !component)
            {
                component = GameObject.GetComponentInChildren<T>();
                if (component) _getComponentInChildren[key] = component;
            }
            return (T)component;
        }
    }
}