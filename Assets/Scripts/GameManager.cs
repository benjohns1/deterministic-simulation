using Game;
using Game.Camera;
using Game.Movement;
using Game.UnitSelection;
using Persistence;
using SimLogic;
using SimpleInjector;
using Simulation;
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

public class GameManager : MonoBehaviour
{
    private Initializer Initializer;
    private TickNumber CurrentTick;
    private bool NewTickThisUpdate;
    private float CurrentFrameTime;
    private IGameState GameState;
    private SimEventEmitter SimEventEmitter;
    private Sim Sim;
    private IPersistence Persist;
    private float TickOffset;
    private string QuicksaveFilename;

    public float SecondsPerSimulationTick = 0.5f;
    public MovementSystem MovementSystem;

    public static Container Container { get => container ?? InitContainer(); }
    private static Container container;

    private Simulation.ILogger Logger;
    private IInputHandler InputHandler;

    // @TODO: Get rid of these as static fields, provide lookup for Components that need to register themselves with the correct systems
    public static CameraSystem CameraSystem;
    public static SelectionSystem SelectionSystem;

    public bool Replaying
    {
        get => _replaying;
        private set
        {
            if (value)
            {
                Initializer.InitializeFromScene();
                SetGameState(Initializer.InitialGameState);
                LastReplayTick = CurrentTick + 1;
                TickOffset = Time.time;
            }

            SimEventEmitter.Enable = !value;
            _replaying = value;
        }
    }

    private uint LastReplayTick;
    private bool _replaying;

    private void Awake()
    {
        QuicksaveFilename = Application.persistentDataPath + "/Saves/quicksave.sav";

        // Instantiate deps for this class
        Logger = Container.GetInstance<Simulation.ILogger>();
        InputHandler = Container.GetInstance<IInputHandler>();
        CameraSystem = Container.GetInstance<CameraSystem>();
        SelectionSystem = Container.GetInstance<SelectionSystem>();
    }

    private static Container InitContainer()
    {
        // Initialize depencency injection container services
        container = new Container();
        container.Options.DefaultLifestyle = Lifestyle.Singleton;
        container.Register<Simulation.ILogger, UnityLogger>();
        container.Register<IInputHandler, InputHandler>();
        container.Register<CameraSystem>();
        container.Register<SelectionSystem>();
        container.Collection.Register(typeof(IRegistrar<>), typeof(IRegistrar<>).Assembly);
        container.Collection.Register<IGameSystem>(typeof(IGameSystem).Assembly);

        container.Verify();
        return container;
    }

    private void OnEnable()
    {
        SetupInputHandler();

        Scene scene = SceneManager.GetActiveScene();
        if (scene != null && scene.isLoaded)
        {
            Initialize();
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
        Initialize();
    }

    private void Initialize(SerializableGame gameData = null)
    {
        Persist = new Filesystem(new Simulation.Serialization.Serializer());
        Initializer = new Initializer(gameData, Assembly.GetExecutingAssembly());
        SetGameState(Initializer.InitialGameState);
        Sim = new Sim(Logger, Initializer.InitialSimState, SimEventEmitter, Initializer.Systems, OnSimUpdated, Initializer.InitialEvents);
        TickOffset = Time.time;
    }

    private void SetGameState(GameState gameState)
    {
        GameState = gameState;
        foreach (IGameSystem system in Container.GetAllInstances<IGameSystem>())
        {
            system.SetGameState(GameState);
        }
        SimEventEmitter = new SimEventEmitter(InputHandler, CameraSystem, SelectionSystem);
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
            IEnumerable<SimSystem> systems = Initializer.InstantiateSimSystems(Assembly.GetExecutingAssembly());
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
        Initialize(data);
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