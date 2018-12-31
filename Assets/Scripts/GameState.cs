using Simulation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ArchetypeName = System.String;
using EntityID = System.UInt64;

class GameState
{
    public struct GameObjectRef
    {
        public readonly GameObject GameObject;
        public readonly ArchetypeName ArchetypeName;

        public GameObjectRef(GameObject gameObject)
        {
            GameObject = gameObject;
            ArchetypeName = GetArchetypeName(gameObject);
        }
    }

    private readonly Dictionary<EntityID, GameObjectRef> Entities = new Dictionary<EntityID, GameObjectRef>();
    private readonly Dictionary<ArchetypeName, GameObject> Archetypes = new Dictionary<ArchetypeName, GameObject>();

    public void AddGameObject(EntityID entityID, GameObject gameObject)
    {
        Entities.Add(entityID, new GameObjectRef(gameObject));
    }

    public GameObjectRef GetGameObject(EntityID EntityID)
    {
        return Entities[EntityID];
    }

    public Dictionary<EntityID, ArchetypeName> GetEntityArchetypes()
    {
        return Entities.ToDictionary(e => e.Key, e => e.Value.ArchetypeName);
    }

    public void AddArchetype(GameObject gameObject)
    {
        ArchetypeName name = GetArchetypeName(gameObject);
        if (!Archetypes.ContainsKey(name))
        {
            Archetypes.Add(name, gameObject);
        }
    }

    private static string GetArchetypeName(GameObject gameObject)
    {
        string name = gameObject.GetComponent<SimEntityComponent>().ArchetypeName;
        return string.IsNullOrWhiteSpace(name) ? gameObject.name : name;
    }

    public GameObject InstantiateArchetypeAndAdd(EntityID entityID, ArchetypeName archetypeName)
    {
        GameObject gameObject = Object.Instantiate(Archetypes[archetypeName]);
        gameObject.name = archetypeName;
        AddGameObject(entityID, gameObject);
        return gameObject;
    }
}