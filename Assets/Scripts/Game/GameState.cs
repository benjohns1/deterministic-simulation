using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ArchetypeName = System.String;
using EntityID = System.UInt64;

namespace Game
{
    public class GameState : IGameState
    {
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
            ArchetypeName name = GameObjectRef.GetArchetypeName(gameObject);
            if (!Archetypes.ContainsKey(name))
            {
                Archetypes.Add(name, gameObject);
            }
        }

        public GameObject InstantiateArchetype(EntityID entityID, ArchetypeName archetypeName)
        {
            GameObject gameObject = Object.Instantiate(Archetypes[archetypeName]);
            gameObject.name = archetypeName;
            return gameObject;
        }
    }
}