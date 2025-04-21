using Moths.Utility;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Moths.Debugging {
    public class GUILogger : MonoBehaviour
    {

#if UNITY_EDITOR
        private const string EnableLoggerMenu = "Moths/Debugger/Enable Logger";
        [UnityEditor.MenuItem(EnableLoggerMenu)]
        private static void ToggleLogger()
        {
            IsEnabled = !IsEnabled;
        }

        [UnityEditor.MenuItem(EnableLoggerMenu, true)]
        private static bool ToggleLoggerValidate()
        {
            UnityEditor.Menu.SetChecked(EnableLoggerMenu, IsEnabled);
            return true;
        }
#endif

        private static bool IsEnabled
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(EnableLoggerPrefs, true);
#endif
                return true;
            }
            set
            {
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool(EnableLoggerPrefs, value);
#endif
            }
        }
        private const string EnableLoggerPrefs = "Moths/Debugger/Enable Logger";

        private static List<ILoggable> _loggables = new List<ILoggable>();


        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            if (IsEnabled)
            {
                GameObject logger = new GameObject("[Moths] GUI Logger", typeof(GUILogger));
            }
        }

        private StringBuilder _builder = new StringBuilder();
        private GUIStyle _style;

        public static void Register(ILoggable loggable)
        {
            if (_loggables.Contains(loggable)) return;
            _loggables.Add(loggable);
        }

        public static void Unregister(ILoggable loggable)
        {
            _loggables.Remove(loggable);
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = GUI.skin.label;
                _style.alignment = TextAnchor.UpperRight;
                _style.fontSize = 16;
                _style.fontStyle = FontStyle.Normal;
            }

            Rect position = new Rect(0, 0, Screen.width - 16, Screen.height);
            _builder.Clear();

            for (int i = 0; i < _loggables.Count; i++)
            {
                if (!_loggables[i].IsLoggable) continue;

                _builder.AppendLine();

                _builder.Append("<b><color=yellow>");
                _builder.Append(_loggables[i].Title);
                _builder.Append("</color></size></b>");

                _builder.AppendLine();
                _builder.Append(_loggables[i].GetString());

                _builder.AppendLine();
            }

            GUI.Label(position, _builder.ToString(), _style);
        }
    }
}