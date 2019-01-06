using Game;
using Game.Camera;
using Game.Movement;
using Game.UnitSelection;
using Persistence;
using SimLogic;
using SimpleInjector;
using Simulation;
using Simulation.ExternalEvent;
using Simulation.State;
using Simulation.TestRunner;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UserInput;
using Utility;
using TickNumber = System.UInt32;
using EntityID = System.UInt64;
using ArchetypeName = System.String;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private TickNumber CurrentTick;
    private bool NewTickThisUpdate;
    private float CurrentFrameTime;
    private IGameState GameState;
    private ISimEventEmitter SimEventEmitter;
    private Sim Sim;
    private IPersistence Persist;
    private float TickOffset;
    private string QuicksaveFilename;

    public float SecondsPerSimulationTick = 0.5f;

    public static Container Container { get => container ?? InitContainer(); }
    private static Container container;

    private Simulation.ILogger Logger;
    private IInputHandler InputHandler;

    public bool Replaying
    {
        get => _replaying;
        private set
        {
            if (value)
            {
                LastReplayTick = CurrentTick + 1;
                TickOffset = Time.time;
            }

            SimEventEmitter.Enable = !value;
            _replaying = value;
        }
    }

    private uint LastReplayTick;
    private bool _replaying;

    private static Container InitContainer()
    {
        // Initialize depencency injection container services
        container = new Container();
        container.Options.DefaultLifestyle = Lifestyle.Singleton;

        // Load default services
        container.Register<Simulation.ILogger>(() => new UnityLogger(UnityLogger.Severity.Debug, new Dictionary<System.Type, UnityLogger.Severity> {
            { typeof(SimEventEmitter), UnityLogger.Severity.Warning }
        }));
        container.Register<IInputHandler, InputHandler>();
        container.Register<CameraSystem>();
        container.Register<SelectionSystem>();
        container.Register<ISerializer, Serializer>();
        container.Register<IPersistence, Filesystem>();
        container.Register<ISimEventEmitter, SimEventEmitter>();

        // Auto-load collections from assembly
        Assembly gameAssembly = Assembly.GetExecutingAssembly();
        container.Collection.Register(typeof(IRegistrar<>), gameAssembly);
        container.Collection.Register<IGameSystem>(gameAssembly);
        container.Collection.Register<ISimSystem>(gameAssembly);

        // Verify container
        container.Verify();
        return container;
    }

    private void Awake()
    {
        QuicksaveFilename = Application.persistentDataPath + "/Saves/quicksave.sav";

        // Instantiate deps for this class
        Logger = Container.GetInstance<Simulation.ILogger>();
        InputHandler = Container.GetInstance<IInputHandler>();
        Persist = Container.GetInstance<IPersistence>();
        SimEventEmitter = Container.GetInstance<ISimEventEmitter>();
    }

    private void OnEnable()
    {
        SetupInputHandler();

        Scene scene = SceneManager.GetActiveScene();
        if (scene != null && scene.isLoaded)
        {
            LoadGame();
        }
        else
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        SetupInputHandler(false);
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        LoadGame();
    }

    private struct State
    {
        public readonly GameState GameState;
        public readonly SimState SimState;

        public State(GameState gameState, SimState simState)
        {
            GameState = gameState;
            SimState = simState;
        }
    }

    private void LoadGame(SerializableGame gameData = null)
    {
        State state = gameData == null ? InitializeFromScene() : InitializeFromGameData(gameData);
        Dictionary<TickNumber, List<IEvent>> initialEvents = gameData == null ? null : gameData.DeserializedEvents;
        IEnumerable<ISimSystem> simSystems = Container.GetAllInstances<ISimSystem>();
        SetGameState(state.GameState);
        Sim = new Sim(Logger, state.SimState, SimEventEmitter, simSystems, OnSimUpdated, initialEvents);
        TickOffset = Time.time;
    }

    private static State InitializeFromScene()
    {
        GameState GameState = new GameState();
        SimState SimState = new SimState();

        SimEntityComponent[] simEntities = FindObjectsOfType<SimEntityComponent>();
        foreach (SimEntityComponent entityComponent in simEntities)
        {
            EntityID entityID = SimState.CreateEntity();
            entityComponent.EntityID = entityID;
            GameState.AddArchetype(entityComponent.gameObject);
            GameState.AddGameObject(entityID, entityComponent.gameObject);

            // @TODO: Move component-specific logic to IComponentInitializer adapters or elsewhere?
            Transform transformComponent = entityComponent.GetComponent<Transform>();
            if (transformComponent != null)
            {
                if (entityComponent.GetComponent<CameraComponent>() == null)
                {
                    SimState.AddInitialComponent(new SimPosition(entityID, transformComponent.position));
                }
            }

            SelectableComponent selectableComponent = entityComponent.GetComponent<SelectableComponent>();
            if (selectableComponent != null)
            {
                SimState.AddInitialComponent(new SimSelectable(entityID, selectableComponent.Selected));
            }

            VelocityComponent velocityComponent = entityComponent.GetComponent<VelocityComponent>();
            if (velocityComponent != null)
            {
                SimState.AddInitialComponent(new SimVelocity(entityID, velocityComponent.Velocity, velocityComponent.MaxAcceleration));
            }
            CameraComponent cameraComponent = entityComponent.GetComponent<CameraComponent>();
            if (cameraComponent != null)
            {
                SimState.AddInitialComponent(new SimCamera(entityID, cameraComponent.transform.position, cameraComponent.enabled));
            }
        }

        return new State(GameState, SimState);
    }

    private State InitializeFromGameData(SerializableGame data)
    {
        return InitializeState(data.InitialSnapshot, data.SnapshotHistory, data.NextEntityID, data.Archetypes);
    }

    private State InitializeState(Snapshot initialSnapshot, Dictionary<TickNumber, Snapshot> snapshotHistory, EntityID nextEntityID, Dictionary<EntityID, ArchetypeName> archetypes)
    {
        GameState GameState = new GameState();
        SimState SimState = new SimState(initialSnapshot, snapshotHistory, nextEntityID);

        SimEntityComponent[] simEntities = FindObjectsOfType<SimEntityComponent>();
        foreach (SimEntityComponent entityComponent in simEntities)
        {
            GameState.AddArchetype(entityComponent.gameObject);
            Destroy(entityComponent.gameObject);
        }

        // Create game objects and save entity in sim
        List<EntityID> createdEntities = new List<EntityID>();
        List<GameObject> createdGameObjects = new List<GameObject>();
        foreach (SimComponent component in SimState.GetComponents())
        {
            if (!createdEntities.Contains(component.EntityID))
            {
                string archetypeName = archetypes.First(a => a.Key == component.EntityID).Value;
                GameObject gameObject = GameState.InstantiateArchetype(component.EntityID, archetypeName);
                GameState.AddGameObject(component.EntityID, gameObject);
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
                GameObject cameraObject = GameState.GetGameObject(component.EntityID).GameObject;
                cameraObject.transform.position = (component as SimCamera).Position;
            }
        }

        return new State(GameState, SimState);
    }

    private void SetGameState(GameState gameState)
    {
        GameState = gameState;
        foreach (IGameSystem system in Container.GetAllInstances<IGameSystem>())
        {
            system.SetGameState(GameState);
        }
    }

    private void SetupInputHandler(bool setup = true)
    {
        if (setup)
        {
            InputHandler.OnKeyEvent += InputHandler_OnKeyEvent;
        }
        else
        {
            InputHandler.OnKeyEvent -= InputHandler_OnKeyEvent;
        }
    }

    private async void InputHandler_OnKeyEvent(KeyEvent keyEvent)
    {
        if (keyEvent.KeyInteraction != Interaction.Pressed)
        {
            return;
        }

        switch (keyEvent.Action)
        {
            case InputAction.QuickSave:
                await QuickSave();
                break;
            case InputAction.QuickLoad:
                QuickLoad();
                break;
            case InputAction.Replay:
                Replay();
                break;
            case InputAction.TestQuickSave:
                RunTest(QuicksaveFilename);
                break;
        }
    }

    private void RunTest(string filename)
    {
        try
        {
            SerializableGame data = Persist.LoadGame(filename);
            IEnumerable<ISimSystem> systems = Container.GetAllInstances<ISimSystem>();
            SimState simState = new SimState(data.InitialSnapshot, data.SnapshotHistory, data.NextEntityID);
            Test test = new Test(Logger, simState, systems, data.DeserializedEvents);
            test.Run();
            Logger.Debug("Test run successfully");
        }
        catch (System.Exception ex)
        {
            Logger.Error(ex);
        }
    }

    private async Task QuickSave()
    {
        await Task.Factory.StartNew(() =>
        {
            Persist.SaveGame(QuicksaveFilename, CurrentTick, Sim, GameState);
        });
    }

    private void QuickLoad()
    {
        SerializableGame data = Persist.LoadGame(QuicksaveFilename);
        LoadGame(data);
    }

    private void Replay()
    {
        Replaying = true;
    }

    private void Update()
    {
        if (Replaying)
        {
            CalculateTick();
            InputHandler.Capture();

            Sim.RunTick(CurrentTick);
            if (CurrentTick > LastReplayTick)
            {
                Replaying = false;
            }
            return;
        }

        float tickOffset = TickOffset;
        CalculateTick();

        // Capture user input and fire events
        InputHandler.Capture();

        // Update game logic
        foreach (IGameSystem system in Container.GetAllInstances<IGameSystem>())
        {
            system.Update(NewTickThisUpdate);
        }

        // Update simulation logic
        if (tickOffset != TickOffset)
        {
            CalculateTick();
        }
        Sim.PlayTick(CurrentTick);
    }

    private void CalculateTick()
    {
        CurrentFrameTime = Time.time - TickOffset;
        TickNumber tick = (TickNumber)Mathf.FloorToInt(CurrentFrameTime / SecondsPerSimulationTick);
        NewTickThisUpdate = tick != CurrentTick;
        CurrentTick = tick;
    }

    private void OnSimUpdated(FrameSnapshot frame)
    {
        float interpolation = InterpolationValue(CurrentFrameTime, frame.Tick, SecondsPerSimulationTick);
        foreach (IGameSystem system in Container.GetAllInstances<IGameSystem>())
        {
            system.OnSimUpdated(frame, interpolation, Replaying);
        }
    }

    private static float InterpolationValue(float time, TickNumber tick, float tickLength)
    {
        float lerpTime = time - (tick * tickLength);
        return Mathf.InverseLerp(0, tickLength, lerpTime);
    }
}