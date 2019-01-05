namespace Simulation
{
    public interface ILogger
    {
        void Debug(object message);
        void Error(object message);
    }
}
