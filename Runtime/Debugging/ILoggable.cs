using System.Text;
using UnityEngine;

namespace Moths.Debugging
{
    public interface ILoggable
    {
        int order => 0;
        bool IsLoggable { get; }
        string Title { get; }
        StringBuilder GetString();
    }
}