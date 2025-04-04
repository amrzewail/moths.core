using Moths.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Fields
{

    [CreateAssetMenu(menuName = "ScriptableObjects/Fields/ULong Field")]
    [PreserveScriptableObject]
    public class ULongField : GenericField<ulong> { }


    [System.Serializable]
    public class ULongReference : GenericReference<ulong, ULongField, ULongMB> { }

}