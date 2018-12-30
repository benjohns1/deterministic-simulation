using Simulation;
using Simulation.ExternalEvent;
using System.Collections.Generic;
using UnityEngine;
using Simulation.State;
using EntityID = System.UInt64;
using TickNumber = System.UInt32;
using System.Reflection;
using System.Linq;

class Initializer
{
    public GameState InitialGameState { get; private set; }
    public SimState InitialSimState { get; private set; }
    public List<SimSystem> Systems { get; }

    Dictionary<TickNumber, List<IEvent>> InitialEvents => null;

    public Initializer(SerializableGameData data, Assembly systemsAssembly)
    {
        InitialGameState = new GameState();
        Systems = InitializeSystems(systemsAssembly);

        if (data == null)
        {
            InitializeFromScene();
        }
        else
        {
            InitializeFromGameData(data);
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
            InitialGameState.AddArchetype(entityComponent.gameObject);
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
                InitialSimState.AddComponent(new SimCamera(entityID, cameraComponent.Speed, cameraComponent.FastSpeed, cameraComponent.enabled));
            }
        }
    }

    private void InitializeFromGameData(SerializableGameData data)
    {
        InitialSimState = new SimState(data.Snapshot);

        EntityComponent[] simEntities = Object.FindObjectsOfType<EntityComponent>();
        foreach (EntityComponent entityComponent in simEntities)
        {
            InitialGameState.AddArchetype(entityComponent.gameObject);
            Object.Destroy(entityComponent.gameObject);
        }

        List<EntityID> createdEntities = new List<EntityID>();
        foreach (SimComponent component in InitialSimState.GetComponents())
        {
            if (!createdEntities.Contains(component.EntityID))
            {
                string archetypeName = data.Archetypes.First(a => a.Key == component.EntityID).Value;
                InitialGameState.InstantiateArchetypeAndAdd(component.EntityID, archetypeName);
                createdEntities.Add(component.EntityID);
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
