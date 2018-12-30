using UnityEngine;
using EntityID = System.UInt64;
using ArchetypeName = System.String;

namespace Simulation
{
    public class EntityComponent : MonoBehaviour
    {
        public EntityID EntityID;
        public ArchetypeName ArchetypeName;
    }
}