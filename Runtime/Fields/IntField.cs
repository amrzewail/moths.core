using Moths.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Fields
{

    [CreateAssetMenu(menuName = "ScriptableObjects/Fields/Int Field")]
    [PreserveScriptableObject]
    public class IntField : GenericField<int> { }


    [System.Serializable]
    public class IntReference : GenericReference<int, IntField, IntMB> { }

}