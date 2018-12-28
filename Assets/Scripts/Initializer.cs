using Simulation;
using Simulation.ExternalEvent;
using System.Collections.Generic;
using UnityEngine;

class Initializer : IInitializer
{
    private readonly SimState InitialSimState;

    public GameState InitialGameState { get; }

    SimState IInitializer.InitialSimState => InitialSimState;
    Dictionary<uint, List<IEvent>> IInitializer.InitialEvents => null;

    public Initializer()
    {
        SimEntityComponent[] simEntities = Object.FindObjectsOfType<SimEntityComponent>();
        InitialSimState = new SimState();
        InitialGameState = new GameState();

        foreach (SimEntityComponent simEntity in simEntities)
        {
            ulong entityID = InitialSimState.CreateNewEntity();
            InitialGameState.AddGameObject(entityID, simEntity.gameObject);

            // @TODO: Move component-specific logic to IComponentInitializer adapters or elsewhere?
            Transform transformComponent = simEntity.GetComponent<Transform>();
            if (transformComponent != null)
            {
                InitialSimState.AddComponent(new SimPosition(entityID, transformComponent.position));
            }

            VelocityComponent velocityComponent = simEntity.GetComponent<VelocityComponent>();
            if (velocityComponent != null)
            {
                InitialSimState.AddComponent(new SimVelocity(entityID, velocityComponent.Velocity, velocityComponent.MaxAcceleration));
            }
            CameraComponent cameraComponent = simEntity.GetComponent<CameraComponent>();
            if (cameraComponent != null)
            {
                InitialSimState.AddComponent(new SimCamera(entityID, cameraComponent.Speed, cameraComponent.FastSpeed));
            }
        }
    }
}
