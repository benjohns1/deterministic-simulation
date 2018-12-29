using System.Collections.Generic;
using UnityEngine;
using EntityID = System.UInt64;

class GameState
{
    private readonly Dictionary<EntityID, GameObject> Entities = new Dictionary<EntityID, GameObject>();

    public void AddGameObject(EntityID entityID, GameObject gameObject)
    {
        Entities.Add(entityID, gameObject);
    }

    public GameObject GetGameObject(EntityID EntityID)
    {
        return Entities[EntityID];
    }
}