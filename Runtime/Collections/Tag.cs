using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Collections
{
    [System.Serializable]
    public struct Tag
    {
        private static Dictionary<string, GameObject> _gameObjects = new Dictionary<string, GameObject>(128);

        [SerializeField] private string tag;

        public string Value { get { return tag; } }

        public Tag(string tag) => this.tag = tag;

        public static implicit operator Tag(string tag) => new Tag(tag);

        public static implicit operator string(Tag tag) => tag.tag;

        public GameObject GameObject => GetGameObject(Value);

        public static GameObject GetGameObject(string tag)
        {
            if (_gameObjects.ContainsKey(tag) && _gameObjects[tag])
            {
                return _gameObjects[tag];
            }
            return _gameObjects[tag] = GameObject.FindGameObjectWithTag(tag);
        }
    }
}