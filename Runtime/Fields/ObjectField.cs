using Moths.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Fields
{

    [CreateAssetMenu(menuName = "Moths/Fields/Object Field")]
    [PreserveScriptableObject]
    public class ObjectField : GenericField<UnityEngine.Object> { }


    [System.Serializable]
    public class ObjectReference : GenericReference<UnityEngine.Object, ObjectField, ObjectMB> { }
}