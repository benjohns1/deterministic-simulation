using GameLogic;
using SimLogic;
using Simulation;
using Simulation.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UserInput;
using TickNumber = System.UInt32;

public class GameManager : MonoBehaviour
{
    private TickNumber CurrentTick;
    private bool NewTickThisUpdate;
    private float CurrentFrameTime;
    private GameState GameState;
    private SimEventEmitter SimEventEmitter;
    private Sim Sim;
    private Updater Updater;
    private float TickOffset;
    private string QuicksaveFilename;

    public float SecondsPerSimulationTick = 0.1f;

    public static InputHandler InputHandler = new InputHandler();
    public static CameraSystem CameraSystem = new CameraSystem(InputHandler);

    private void OnEnable()
    {
        QuicksaveFilename = Application.persistentDataPath + "/Saves/quicksave.sav";
        SetupInputHandler();

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        Scene scene = SceneManager.GetActiveScene();
        if (scene != null && scene.isLoaded)
        {
            Initialize();
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

    private void Initialize(string loadFromFile = null)
    {
        SerializableGameData data = loadFromFile == null ? null : LoadFromFile(QuicksaveFilename);
        Initializer initializer = new Initializer(data, Assembly.GetExecutingAssembly());
        GameState = initializer.InitialGameState;
        Updater = new Updater(GameState);
        SimEventEmitter = new SimEventEmitter(InputHandler, CameraSystem);
        Sim = new Sim(initializer.InitialSimState, SimEventEmitter, initializer.Systems, OnSimUpdated);

        TickOffset = Time.time;
        Sim.Enabled = true;
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

    private void InputHandler_OnKeyEvent(KeyEvent keyEvent)
    {
        if (keyEvent.KeyInteraction != KeyInteraction.Pressed)
        {
            return;
        }

        switch (keyEvent.Action)
        {
            case InputAction.QuickSave:
                QuickSave();
                break;
            case InputAction.QuickLoad:
                QuickLoad();
                break;
        }
    }

    private async void QuickSave()
    {
        await Task.Factory.StartNew(() =>
        {
            SaveToFile(QuicksaveFilename);
        });
    }

    private void QuickLoad()
    {
        Sim.Enabled = false;
        Initialize(QuicksaveFilename);
    }

    private void SaveToFile(string Filename)
    {
        (new FileInfo(Filename)).Directory.Create();
        using (FileStream fileStream = new FileStream(Filename, FileMode.Create, FileAccess.Write))
        {
            Snapshot snapshot = Sim.GetSnapshot(CurrentTick);
            Dictionary<ulong, string> archetypes = GameState.GetEntityArchetypes();
            SerializableGameData data = new SerializableGameData(snapshot, archetypes);
            data.Serialize(fileStream);
        }
    }

    private SerializableGameData LoadFromFile(string Filename)
    {
        using (FileStream stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
        {
            return SerializableGameData.Deserialize(stream);
        }
    }

    private void Update()
    {
        float tickOffset = TickOffset;
        CalculateTick();

        // Capture user input and fire events
        InputHandler.Update();

        // Update game logic
        CameraSystem.Update(NewTickThisUpdate);

        // Update simulation logic
        if (tickOffset != TickOffset)
        {
            CalculateTick();
        }
        Sim.Update(CurrentTick);
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
        Updater.UpdateGame(frame, interpolation);
    }

    private static float InterpolationValue(float time, TickNumber tick, float tickLength)
    {
        float lerpTime = time - (tick * tickLength);
        return Mathf.InverseLerp(0, tickLength, lerpTime);
    }
}
