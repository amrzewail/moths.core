using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Attributes
{

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class PreserveScriptableObjectAttribute : System.Attribute
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class PersistScriptableObjectField : System.Attribute
    {

    }
}