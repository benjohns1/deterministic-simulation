using Simulation;
using Simulation.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UserInput;
using TickNumber = System.UInt32;

public class GameManager : MonoBehaviour
{
    private TickNumber CurrentTick;
    private float CurrentFrameTime;
    private GameState GameState;
    private Sim Sim;
    private Updater Updater;
    private InputHandler InputHandler;
    private float TickOffset;
    private string QuicksaveFilename;

    public float SecondsPerSimulationTick = 0.1f;

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
        Sim = new Sim(initializer.InitialSimState, InputHandler, initializer.Systems, OnSimUpdated);

        TickOffset = Time.time;
        Sim.Enabled = true;
    }

    private void SetupInputHandler(bool setup = true)
    {
        if (setup)
        {
            InputHandler = new InputHandler();
            InputHandler.OnFunctionKeyEvent += InputHandler_OnFunctionKeyEvent;
        }
        else
        {
            InputHandler.OnFunctionKeyEvent -= InputHandler_OnFunctionKeyEvent;
        }
    }

    private void InputHandler_OnFunctionKeyEvent(FunctionKeyEvent @event)
    {
        switch (@event.FunctionAction)
        {
            case FunctionAction.QuickSave:
                QuickSave();
                break;
            case FunctionAction.QuickLoad:
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
            Snapshot snapshot = Sim.GetSnapshot(Math.Max(CurrentTick - 1, 0));
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
        InputHandler.Capture();
        CurrentFrameTime = Time.time - TickOffset;
        CurrentTick = (TickNumber)Mathf.FloorToInt(CurrentFrameTime / SecondsPerSimulationTick);
        Sim.Update(CurrentTick);
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
