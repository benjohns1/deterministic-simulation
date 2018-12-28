using SimAdapters;
using Simulation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UserInput;

public class GameManager : MonoBehaviour
{
    private Sim Sim;
    private Updater Updater;
    private InputHandler InputHandler;

    [SerializeField]
#pragma warning disable IDE0044 // Add readonly modifier
    private float SecondsPerSimulationTick = 0.1f;
#pragma warning restore IDE0044 // Add readonly modifier

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        Scene scene = SceneManager.GetActiveScene();
        if (scene != null && scene.isLoaded)
        {
            SceneManager_sceneLoaded(scene, LoadSceneMode.Single);
        }
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        Initializer initializer = new Initializer();
        InputHandler = new InputHandler();
        Updater = new Updater(initializer.InitialGameState);
        List<SimSystem> systems = new List<SimSystem>()
        {
            new SimAdapters.Camera()
        };
        Sim = new Sim(initializer, InputHandler, systems, OnSimUpdated);
    }

    private void Update()
    {
        uint tick = (uint)Mathf.FloorToInt(Time.time / SecondsPerSimulationTick);
        InputHandler.Capture();
        Sim.Update(tick);
    }

    private void OnSimUpdated(uint tick, bool stateUpdated, SimState State)
    {
        float interpolation = InterpolationValue(Time.time, tick, SecondsPerSimulationTick);
        Updater.UpdateGame(State, interpolation);
    }

    private static float InterpolationValue(float time, uint tick, float tickLength)
    {
        float tickTime = tick * tickLength;
        return Mathf.InverseLerp(tickTime, tickTime + tickLength, time);
    }
}
