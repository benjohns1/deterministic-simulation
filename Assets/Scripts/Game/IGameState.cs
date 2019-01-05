using System.Collections.Generic;
using UnityEngine;
using ArchetypeName = System.String;
using EntityID = System.UInt64;

namespace Game
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

        public static ArchetypeName GetArchetypeName(GameObject gameObject)
        {
            string name = gameObject.GetComponent<Simulation.SimEntityComponent>().ArchetypeName;
            return string.IsNullOrWhiteSpace(name) ? gameObject.name : name;
        }
    }

    public interface IGameState
    {
        void AddGameObject(EntityID entityID, GameObject gameObject);
        GameObjectRef GetGameObject(EntityID EntityID);
        Dictionary<EntityID, ArchetypeName> GetEntityArchetypes();
        void AddArchetype(GameObject gameObject);
        GameObject InstantiateArchetype(EntityID entityID, ArchetypeName archetypeName);
    }
}