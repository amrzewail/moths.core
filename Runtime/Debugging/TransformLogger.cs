using System.Text;
using UnityEngine;

namespace Moths.Debugging
{
    public class TransformLogger : GUILoggable
    {
        protected override void OnLog(StringBuilder builder)
        {
            builder.AppendLine($"Position: {transform.position}");
            builder.AppendLine($"Rotation: {transform.eulerAngles}");
            builder.AppendLine($"Scale: {transform.localScale}");
        }
    }
}