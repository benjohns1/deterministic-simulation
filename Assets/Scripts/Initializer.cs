using Simulation;
using Simulation.ExternalEvent;
using System.Collections.Generic;
using UnityEngine;
using Simulation.State;
using EntityID = System.UInt64;
using TickNumber = System.UInt32;
using System.Reflection;
using System.Linq;

class Initializer : IInitializer
{
    public GameState InitialGameState { get; private set; }
    public SimState InitialSimState { get; private set; }
    public List<SimSystem> Systems { get; }

    Dictionary<TickNumber, List<IEvent>> IInitializer.InitialEvents => null;

    public Initializer(string loadFromFile, Assembly systemsAssembly)
    {
        InitialGameState = new GameState();
        Systems = InitializeSystems(systemsAssembly);

        if (loadFromFile == null)
        {
            InitializeFromScene();
        }
        else
        {
            InitializeFromFile(loadFromFile);
        }


    }

    private void InitializeFromScene()
    {
        InitialSimState = new SimState();

        EntityComponent[] simEntities = Object.FindObjectsOfType<EntityComponent>();
        foreach (EntityComponent entityComponent in simEntities)
        {
            EntityID entityID = InitialSimState.CreateEntity();
            entityComponent.EntityID = entityID;
            InitialGameState.AddGameObject(entityID, entityComponent.gameObject);

            // @TODO: Move component-specific logic to IComponentInitializer adapters or elsewhere?
            Transform transformComponent = entityComponent.GetComponent<Transform>();
            if (transformComponent != null)
            {
                InitialSimState.AddComponent(new SimPosition(entityID, transformComponent.position));
            }

            VelocityComponent velocityComponent = entityComponent.GetComponent<VelocityComponent>();
            if (velocityComponent != null)
            {
                InitialSimState.AddComponent(new SimVelocity(entityID, velocityComponent.Velocity, velocityComponent.MaxAcceleration));
            }
            CameraComponent cameraComponent = entityComponent.GetComponent<CameraComponent>();
            if (cameraComponent != null)
            {
                InitialSimState.AddComponent(new SimCamera(entityID, cameraComponent.Speed, cameraComponent.FastSpeed));
            }
        }
    }

    private void InitializeFromFile(string filename)
    {
        InitialSimState = new SimState(filename);

        EntityComponent[] simEntities = Object.FindObjectsOfType<EntityComponent>();
        foreach (EntityComponent entityGameObject in simEntities)
        {
            Object.Destroy(entityGameObject.gameObject);
        }

        foreach (SimComponent component in InitialSimState.GetComponents())
        {
            System.Type type = component.GetType();
            Debug.Log("Loading " + type);
            if (type == typeof(SimPosition))
            {

            }
        }
    }

    private static List<SimSystem> InitializeSystems(Assembly assembly)
    {
        List<SimSystem> systems = new List<SimSystem>();
        System.Type parentType = typeof(SimSystem);
        foreach (System.Type systemType in assembly.GetTypes().Where(t => parentType.IsAssignableFrom(t)))
        {
            systems.Add((SimSystem)System.Activator.CreateInstance(systemType));
        }
        return systems;
    }
}
