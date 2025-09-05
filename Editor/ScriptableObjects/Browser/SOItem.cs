using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Moths.ScriptableObjects.Browser
{
    class SOItem : TreeViewItem
    {
        private ScriptableObject _target;

        public string guid;
        public bool isFavourite;
        public bool isSubAsset;
        public ScriptableObject target
        {
            get => _target;
            set
            {
                _target = value;
                assetPath = AssetDatabase.GetAssetPath(value);
            }
        }

        public string assetPath;
    }
}