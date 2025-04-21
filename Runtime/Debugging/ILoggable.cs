using System.Text;
using UnityEngine;

namespace Moths.Debugging
{
    public interface ILoggable
    {
        bool IsLoggable { get; }
        string Title { get; }
        StringBuilder GetString();
    }
}