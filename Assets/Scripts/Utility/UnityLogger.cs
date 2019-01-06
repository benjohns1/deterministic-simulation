using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Utility
{
    public class UnityLogger : Simulation.ILogger
    {
        public enum Severity { Debug, Warning, Error }

        private readonly Severity GlobalSeverity;

        private readonly IDictionary<System.Type, Severity> LogLevels;

        private delegate void LogDelegate(object message);

        public UnityLogger(Severity globalSeverity, IDictionary<System.Type, Severity> logLevels = null)
        {
            GlobalSeverity = globalSeverity;
            LogLevels = logLevels ?? new Dictionary<System.Type, Severity>();
        }

        public void Debug(object message)
        {
            Log(message, Severity.Debug, UnityEngine.Debug.Log);
        }

        public void Warning(object message)
        {
            Log(message, Severity.Warning, UnityEngine.Debug.LogWarning);
        }

        public void Error(object message)
        {
            Log(message, Severity.Error, UnityEngine.Debug.LogError);
        }

        private void Log(object message, Severity severity, LogDelegate logMethod)
        {
            if (GlobalSeverity > severity)
            {
                return;
            }
            MethodBase method = (new StackFrame(2)).GetMethod();
            if (!ShouldLog(method, severity))
            {
                return;
            }
            logMethod.Invoke(GetPrefix(method) + message);
        }

        private static string GetPrefix(MethodBase callingMethod)
        {
            return callingMethod.DeclaringType.Namespace + "." + callingMethod.DeclaringType.Name + "." + callingMethod.Name + ": ";
        }

        private static MethodBase GetCallingMethodBase(int skip)
        {
            StackFrame frame = new StackFrame(skip);
            return frame.GetMethod();
        }

        private bool ShouldLog(MethodBase callingMethod, Severity severity)
        {
            System.Type type = callingMethod.DeclaringType;
            if (!LogLevels.ContainsKey(type))
            {
                return true;
            }
            return LogLevels[type] < severity;
        }
    }
}
