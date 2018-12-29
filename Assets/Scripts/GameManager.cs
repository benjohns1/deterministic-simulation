using Simulation;
using Simulation.State;
using Simulation.Serialization;
using System;
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
    private Sim Sim;
    private Updater Updater;
    private InputHandler InputHandler;
    private float TickOffset;
    private string QuicksaveFilename;

    public float SecondsPerSimulationTick = 0.1f;
    public bool LoadFromAutosave;

    private void OnEnable()
    {
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
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        Initialize();
    }

    private void Initialize()
    {
        QuicksaveFilename = Application.persistentDataPath + "/Saves/quicksave.sav";
        string loadFromFile = LoadFromAutosave ? QuicksaveFilename : null;
        Initializer initializer = new Initializer(loadFromFile, Assembly.GetExecutingAssembly());
        InputHandler = new InputHandler();
        Updater = new Updater(initializer.InitialGameState);
        Sim = new Sim(initializer.InitialSimState, InputHandler, initializer.Systems, OnSimUpdated);
        TickOffset = Time.time;

        InputHandler.OnFunctionKeyEvent += InputHandler_OnFunctionKeyEvent;
    }

    private void InputHandler_OnFunctionKeyEvent(FunctionKeyEvent @event)
    {
        switch (@event.FunctionAction)
        {
            case FunctionAction.QuickSave:
                QuickSave();
                break;
        }
    }

    private async void QuickSave()
    {
        await Task.Factory.StartNew(() =>
        {
            (new FileInfo(QuicksaveFilename)).Directory.Create();
            using (FileStream fileStream = new FileStream(QuicksaveFilename, FileMode.Create, FileAccess.Write))
            {
                Snapshot snapshot = Sim.GetSnapshot(Math.Max(CurrentTick - 10, 0));
                // @TODO: merge simulation snapshot with gamestate / prefabs for re-loading entities
                Serializer serializer = new Serializer();
                serializer.Serialize(fileStream, snapshot);
            }
        });
    }

    private void Update()
    {
        CurrentTick = (TickNumber)Mathf.FloorToInt((Time.time - TickOffset) / SecondsPerSimulationTick);
        InputHandler.Capture();
        Sim.Update(CurrentTick);
    }

    private void OnSimUpdated(FrameSnapshot frame)
    {
        float interpolation = InterpolationValue(Time.time, frame.Tick, SecondsPerSimulationTick);
        Updater.UpdateGame(frame, interpolation);
    }

    private static float InterpolationValue(float time, TickNumber tick, float tickLength)
    {
        float tickTime = tick * tickLength;
        return Mathf.InverseLerp(tickTime, tickTime + tickLength, time);
    }
}
