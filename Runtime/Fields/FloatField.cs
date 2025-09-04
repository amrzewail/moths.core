using Moths.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Fields
{
    [CreateAssetMenu(menuName = "Moths/Fields/Float Field")]
    [PreserveScriptableObject]
    public class FloatField : GenericField<float> { }


    [System.Serializable]
    public class FloatReference : GenericReference<float, FloatField, FloatMB> { }

}