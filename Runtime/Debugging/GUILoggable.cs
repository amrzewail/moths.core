using System.Text;
using UnityEngine;

namespace Moths.Debugging
{
    public abstract class GUILoggable : MonoBehaviour, ILoggable
    {
        public bool IsLoggable => enabled && GetIsLoggable();

        public virtual string Title => this.name;

        private StringBuilder _builder = new StringBuilder();

        protected virtual void OnEnable()
        {
            GUILogger.Register(this);
        }

        protected virtual void OnDisable()
        {
            GUILogger.Unregister(this);
        }

        public StringBuilder GetString()
        {
            _builder.Clear();
            OnLog(_builder);
            return _builder;
        }
		
		protected virtual bool GetIsLoggable() => true;

        protected abstract void OnLog(StringBuilder builder);
    }
}