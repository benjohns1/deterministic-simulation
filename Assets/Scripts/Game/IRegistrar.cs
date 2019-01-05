namespace Game
{
    public interface IRegistrar<Registerable> where Registerable : IRegisterable
    {
        void Register<R>(R registerable) where R : Registerable;
        void Unregister<R>(R registerable) where R : Registerable;
    }
}
