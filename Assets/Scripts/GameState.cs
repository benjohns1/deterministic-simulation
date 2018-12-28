using System.Collections.Generic;
using UnityEngine;

class GameState
{
    private readonly Dictionary<ulong, GameObject> Entities = new Dictionary<ulong, GameObject>();

    public void AddGameObject(ulong entityID, GameObject gameObject)
    {
        Entities.Add(entityID, gameObject);
    }

    public GameObject GetGameObject(ulong EntityID)
    {
        return Entities[EntityID];
    }
}