using Simulation;
using Simulation.ExternalEvent;
using System.Collections.Generic;
using UnityEngine;
using Simulation.State;
using EntityID = System.UInt64;
using TickNumber = System.UInt32;
using System.Reflection;
using System.Linq;
using SimLogic;
using Persistence;
using Game.Camera;
using Game.Movement;

public class Initializer
{
    public GameState InitialGameState { get; private set; }
    public SimState InitialSimState { get; private set; }
    public Dictionary<TickNumber, List<IEvent>> InitialEvents { get; private set; }
    public List<SimSystem> Systems { get; }

    public Initializer(SerializableGame data, Assembly systemsAssembly)
    {
        Systems = InstantiateSimSystems(systemsAssembly);

        if (data == null)
        {
            InitializeFromScene();
        }
        else
        {
            InitializeFromGameData(data);
        }
    }

    public void InitializeFromScene()
    {
        InitialGameState = new GameState();
        InitialSimState = new SimState();

        SimEntityComponent[] simEntities = Object.FindObjectsOfType<SimEntityComponent>();
        foreach (SimEntityComponent entityComponent in simEntities)
        {
            EntityID entityID = InitialSimState.CreateEntity();
            entityComponent.EntityID = entityID;
            InitialGameState.AddArchetype(entityComponent.gameObject);
            InitialGameState.AddGameObject(entityID, entityComponent.gameObject);

            // @TODO: Move component-specific logic to IComponentInitializer adapters or elsewhere?
            Transform transformComponent = entityComponent.GetComponent<Transform>();
            if (transformComponent != null)
            {
                if (entityComponent.GetComponent<CameraComponent>() == null)
                {
                    InitialSimState.AddInitialComponent(new SimPosition(entityID, transformComponent.position));
                }
            }

            VelocityComponent velocityComponent = entityComponent.GetComponent<VelocityComponent>();
            if (velocityComponent != null)
            {
                InitialSimState.AddInitialComponent(new SimVelocity(entityID, velocityComponent.Velocity, velocityComponent.MaxAcceleration));
            }
            CameraComponent cameraComponent = entityComponent.GetComponent<CameraComponent>();
            if (cameraComponent != null)
            {
                InitialSimState.AddInitialComponent(new SimCamera(entityID, cameraComponent.transform.position, cameraComponent.enabled));
            }
        }
    }

    public void InitializeFromGameData(SerializableGame data)
    {
        InitialGameState = new GameState();
        InitialSimState = new SimState(data.InitialSnapshot, data.SnapshotHistory, data.NextEntityID);

        SimEntityComponent[] simEntities = Object.FindObjectsOfType<SimEntityComponent>();
        foreach (SimEntityComponent entityComponent in simEntities)
        {
            InitialGameState.AddArchetype(entityComponent.gameObject);
            Object.Destroy(entityComponent.gameObject);
        }

        // Create game objects and save entity in sim
        List<EntityID> createdEntities = new List<EntityID>();
        List<GameObject> createdGameObjects = new List<GameObject>();
        foreach (SimComponent component in InitialSimState.GetComponents())
        {
            if (!createdEntities.Contains(component.EntityID))
            {
                string archetypeName = data.Archetypes.First(a => a.Key == component.EntityID).Value;
                GameObject gameObject = InitialGameState.InstantiateArchetypeAndAdd(component.EntityID, archetypeName);
                createdEntities.Add(component.EntityID);

                // Enable all game components
                // @TODO: save/load 'enabled' automatically upon serialization for each component
                foreach (Behaviour behaviour in gameObject.GetComponents<Behaviour>())
                {
                    behaviour.enabled = true;
                }
            }

            // Logic for any specialized game object initialization
            if (component.GetType() == typeof(SimCamera))
            {
                GameObject cameraObject = InitialGameState.GetGameObject(component.EntityID).GameObject;
                cameraObject.transform.position = (component as SimCamera).Position;
            }
        }

        // Initialize events
        InitialEvents = data.DeserializedEvents;
    }

    public static List<SimSystem> InstantiateSimSystems(Assembly assembly)
    {
        List<SimSystem> systems = new List<SimSystem>();
        System.Type parentType = typeof(SimSystem);
        foreach (System.Type systemType in assembly.GetTypes().Where(t => parentType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract))
        {
            systems.Add((SimSystem)System.Activator.CreateInstance(systemType));
        }
        return systems;
    }
}
