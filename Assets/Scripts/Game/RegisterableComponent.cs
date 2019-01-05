using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class RegisterableComponent : MonoBehaviour, IRegisterable { }

    public abstract class RegisterableComponent<Registerable> : RegisterableComponent where Registerable : RegisterableComponent
    {
        protected virtual IEnumerable<IRegistrar<Registerable>> Registrars => GameManager.Container.GetAllInstances<IRegistrar<Registerable>>();

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            Unregister();
        }

        protected virtual void Register()
        {
            foreach (IRegistrar<Registerable> registrar in Registrars)
            {
                registrar.Register(this as Registerable);
            }
        }

        protected virtual void Unregister()
        {
            foreach (IRegistrar<Registerable> registrar in Registrars)
            {
                registrar.Unregister(this as Registerable);
            }
        }
    }
}
