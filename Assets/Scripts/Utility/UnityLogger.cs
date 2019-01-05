namespace Utility
{
    public class UnityLogger : Simulation.ILogger
    {
        public void Debug(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Error(object message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}
