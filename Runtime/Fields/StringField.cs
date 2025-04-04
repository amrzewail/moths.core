using Moths.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Moths.Fields
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Fields/String Field")]
    [PreserveScriptableObject]
    public class StringField : GenericField<string>
    {
        [Space]
        [Header("Custom Properties")]
        [SerializeField] bool sameAsName;
        [SerializeField] int nameSubstringStartIndex = 0;
        [SerializeField] int nameSubstringLength = 0;

        protected override void OnValidate()
        {

            if (sameAsName)
            {
                nameSubstringStartIndex = Mathf.Clamp(nameSubstringStartIndex, 0, name.Length - 1);
                nameSubstringLength = Mathf.Max(Mathf.Min(nameSubstringLength, name.Length - nameSubstringStartIndex), 0);
                value = this.name.Substring(nameSubstringStartIndex, nameSubstringLength > 0 ? nameSubstringLength : this.name.Length - nameSubstringStartIndex);
            }


            base.OnValidate();
        }
    }


    [System.Serializable]
    public class StringReference : GenericReference<string, StringField, StringMB> { }

}